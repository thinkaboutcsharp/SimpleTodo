using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using stt = System.Threading.Tasks;

using PCLStorage;
using Realms;
using AutoMapper;

namespace SimpleTodo.Realm
{
    public class RealmAccess : IDataAccess
    {
        private RealmConfigurationBase commonConfig;
        private Transaction transaction;
        private Dictionary<int, Realms.Realm> connectionPool = new Dictionary<int, Realms.Realm>();
        private int primaryThreadId;
        private Realms.Realm primaryRealm;

        private string dbFile;

        #region Master Table
        private SystemSettings systemSettings;
        private MenuIconMaster menuIconMaster;
        private IRealmCollection<IconPatternMaster> iconPatternMaster;
        private IRealmCollection<ColorPatternMaster> colorPatternMaster;
        private IRealmCollection<TaskOrderDisplayName> taskOrderDisplayNames;
        #endregion

        #region Stored Information
        private LastPage lastPage;
        #endregion

        #region Connect
        public RealmAccess()
        {
            InitializeRealm();
        }

        public void CloseConnection()
        {
            foreach (var realm in connectionPool.Values)
            {
                primaryRealm.Dispose();
            }
            connectionPool.Clear();
        }

        private void InitializeRealm()
        {
            const int CurrentSchemaVertion = 0; //これを間違うと死ぬ！

            var folder = FileSystem.Current.LocalStorage;
            dbFile = Path.Combine(folder.Path, "item.realm");
            commonConfig = MakeRealmConfiguration(CurrentSchemaVertion, this.GetType().Assembly.GetName().Version.ToString());
            OpenConnection();
        }

        public void OpenConnection()
        {
            if (!File.Exists(dbFile))
            {
                CopyEmbeddedDb(dbFile);
            }
            else
            {
                CloseConnection();
            }

            primaryThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId; //UIスレッドだと仮定する
            primaryRealm = Realms.Realm.GetInstance(commonConfig);
            connectionPool.Add(primaryThreadId, primaryRealm);

            SelectCommonMaster();
        }

        public void OpenParallelConnection(int threadId)
        {
            if (connectionPool.TryGetValue(threadId, out _))
            {
                return;
            }
            var realm = Realms.Realm.GetInstance(commonConfig);
            connectionPool.Add(threadId, realm);
        }

        public void DisposeConnection(int threadId)
        {
            if (connectionPool.TryGetValue(threadId, out var realm))
            {
                realm.Dispose();
                connectionPool.Remove(threadId);
            }
        }
        #endregion

        #region Get/Set Value
        public string GetSystemVersion()
        {
            return primaryRealm.All<SystemVersion>().First().Version;
        }

        public string GetMenuBarIconFile(MenuBarIcon menu)
        {
            switch (menu)
            {
                case MenuBarIcon.TabList:
                    return menuIconMaster.TabListIcon;
                case MenuBarIcon.NewTask:
                    return menuIconMaster.NewTaskIcon;
                case MenuBarIcon.Up:
                    return menuIconMaster.Up;
                case MenuBarIcon.Down:
                    return menuIconMaster.Down;
                case MenuBarIcon.TabSetting:
                    return menuIconMaster.TabSetting;
                case MenuBarIcon.NewTab:
                    return menuIconMaster.NewTabIcon;
                default:
                    return null;
            }
        }

        public bool GetDefaultUseTristate() => systemSettings.DefaultUseTristate;
        public async stt.Task SetDefaultUseTristateAsync(bool value) => await primaryRealm.WriteAsync((realmAsync) => systemSettings.DefaultUseTristate = value);

        public TaskOrderPattern GetDefaultTaskOrder() => systemSettings.DefaultTaskOrder.EnumValue<TaskOrderPattern>();
        public async stt.Task SetDefaultTaskOrderAsync(TaskOrderPattern order) => await primaryRealm.WriteAsync((realmAsync) => systemSettings.DefaultTaskOrder = order.EnumName());

        public bool IsBigIcon() => systemSettings.UseBigIcon;
        public async stt.Task UseBigIconAsync(bool usage) => await primaryRealm.WriteAsync((realmAsync) => systemSettings.UseBigIcon = usage);

        public bool IsBeginFromTabList() => systemSettings.BeginFromTabList;
        public async stt.Task BeginFromTabListAsync(bool begin) => await primaryRealm.WriteAsync((realmAsync) => systemSettings.BeginFromTabList = begin);

        public MenuBarPosition GetMenuBarPosition() => systemSettings.HorizontalMenuBarPosition.EnumValue<MenuBarPosition>();
        public async stt.Task SetMenuBarPositionAsync(MenuBarPosition position) => await primaryRealm.WriteAsync((realmAsync) => systemSettings.HorizontalMenuBarPosition = position.EnumName());

        public TabPosition GetNewTabPosition() => systemSettings.NewTabPosition.EnumValue<TabPosition>();
        public async stt.Task SetNewTabPositionAsync(TabPosition position) => await primaryRealm.WriteAsync((realmAsync) => systemSettings.NewTabPosition = position.EnumName());

        public ViewingPage GetLastPage() => lastPage.LastViewing.EnumValue<ViewingPage>();
        public async stt.Task SetLastPageAsync(ViewingPage viewing) => await primaryRealm.WriteAsync((realmAsync) => lastPage.LastViewing = viewing.EnumName());

        public int GetLastTabIndex() => lastPage.LastFocus;
        public async stt.Task SetLastTabIndexAsync(int focus) => await primaryRealm.WriteAsync((realmAsync) => lastPage.LastFocus = focus);

        public int GetOriginTabIndex() => lastPage.Origin;
        public async stt.Task SetOriginTabIndexAsync(int origin) => await primaryRealm.WriteAsync((realmAsync) => lastPage.Origin = origin);

        public IReadOnlyList<TaskOrderList> GetTaskOrderList()
            => taskOrderDisplayNames.OrderBy(o => (int)o.TaskOrder.EnumValue<TaskOrderPattern>()).Select(o => Mapper.Map<TaskOrderList>(o)).ToList();

        public int GetNewTodoId()
        {
            //これは非同期にできない

            int newId = 0;
            using (var transaction = primaryRealm.BeginWrite())
            {
                var next = primaryRealm.All<TodoIdMaster>().FirstOrDefault();
                if (next != null)
                {
                    newId = next.NextTodoId;
                    next.NextTodoId = newId++;
                }
                else
                {
                    primaryRealm.Add(new TodoIdMaster { NextTodoId = newId + 1 });
                }
                transaction.Commit();
            }
            return newId;
        }

        public int GetNewTaskId(int todoId)
        {
            //これは非同期にできない

            int newId = 0;
            using (var transaction = primaryRealm.BeginWrite())
            {
                var next = primaryRealm.All<TaskIdMaster>().Where(m => m.TodoId == todoId).FirstOrDefault();
                if (next != null)
                {
                    newId = next.NextTaskId;
                    next.NextTaskId = newId + 1;
                }
                else
                {
                    primaryRealm.Add(new TaskIdMaster { TodoId = todoId, NextTaskId = newId + 1 });
                }
                transaction.Commit();
            }
            return newId;
        }
        #endregion

        #region Transaction
        public void BeginTransaction() => transaction = primaryRealm.BeginWrite();
        public void Rollback() { transaction?.Rollback(); transaction = null; }
        public void Commit() { transaction?.Commit(); transaction = null; }
        #endregion

        #region Todo Task
        public async stt.Task<TodoItem> AddTodoAsync(TodoItem todoItem)
        {
            var todo = Mapper.Map<Todo>(todoItem);
            await primaryRealm.WriteAsync((realmAsync) => realmAsync.Add(todo));
            return todoItem;
        }

        public async stt.Task<TodoTask> AddTaskAsync(int todoId, TodoTask todoTask)
        {
            var task = Mapper.Map<Task>(todoTask);
            task.TodoId = todoId;
            await primaryRealm.WriteAsync((realmAsync) => realmAsync.Add(task));
            return todoTask;
        }

        public async stt.Task ChangeVisibilityAsync(int todoId, bool visibile)
        {
            var todo = primaryRealm.All<Todo>().Where(t => t.TodoId == todoId).First();
            await primaryRealm.WriteAsync((realmAsync) => todo.IsActive = visibile);
        }

        public async stt.Task RenameTodoAsync(int todoId, string newName)
        {
            var todo = primaryRealm.All<Todo>().Where(t => t.TodoId == todoId).First();
            await primaryRealm.WriteAsync((realmAsync) => todo.Name = newName);
        }

        public async stt.Task RenameTaskAsync(int todoId, int taskId, string newName)
        {
            var task = primaryRealm.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == taskId).First();
            await primaryRealm.WriteAsync((realmAsync) => task.Name = newName);
        }

        public async stt.Task ReorderTodoAsync(IEnumerable<TodoItem> todoItems)
        {
            var allTodo = primaryRealm.All<Todo>();
            int index = 0;
            await primaryRealm.WriteAsync((realmAsync) =>
            {
                foreach (var todoItem in todoItems)
                {
                    var todo = allTodo.Where(t => t.TodoId == todoItem.TodoId.Value).First();
                    todo.DisplayOrder = index;
                    index++;
                }
            });
        }

        public async stt.Task ReorderTaskAsync(int todoId, IEnumerable<TodoTask> todoTasks)
        {
            var allTask = primaryRealm.All<Task>().Where(t => t.TodoId == todoId);
            int index = 0;
            await primaryRealm.WriteAsync((realmAsync) =>
            {
                foreach (var todoTask in todoTasks)
                {
                    var task = allTask.Where(t => t.TaskId == todoTask.TaskId.Value).First();
                    task.DisplayOrder = index;
                    index++;
                }
            });
        }

        public async stt.Task ToggleTaskStatusAsync(int todoId, int taskId, TaskStatus status)
        {
            var task = primaryRealm.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == taskId).First();
            await primaryRealm.WriteAsync((realmAsync) => task.Status = status.EnumName());
        }

        public async stt.Task UpdateTodoAsync(TodoItem todoItem)
        {
            var todo = primaryRealm.All<Todo>().Where(t => t.TodoId == todoItem.TodoId.Value).First();
            await primaryRealm.WriteAsync((realmAsync) => Mapper.Map(todoItem, todo));
        }

        public async stt.Task UpdateTaskAsync(int todoId, TodoTask todoTask)
        {
            var task = primaryRealm.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == todoTask.TaskId.Value).First();
            await primaryRealm.WriteAsync((realmAsync) => Mapper.Map(todoTask, task));
        }

        public async stt.Task DeleteTodoAsync(int todoId)
        {
            var todo = primaryRealm.All<Todo>().Where(t => t.TodoId == todoId).First();
            await primaryRealm.WriteAsync((realmAsync) => realmAsync.Remove(todo));
        }

        public async stt.Task DeleteTaskAsync(int todoId, int taskId)
        {
            var task = primaryRealm.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == taskId).First();
            await primaryRealm.WriteAsync((realmAsync) => realmAsync.Remove(task));
        }
        #endregion

        #region Query
        public IReadOnlyList<IconSetting> GetIconPatternAll()
        {
            return Mapper.Map<IReadOnlyList<IconPatternMaster>, IReadOnlyList<IconSetting>>(iconPatternMaster);
        }

        public IReadOnlyList<ColorSetting> GetColorPatternAll()
        {
            return Mapper.Map<IReadOnlyList<ColorPatternMaster>, IReadOnlyList<ColorSetting>>(colorPatternMaster);
        }

        public IReadOnlyList<TaskOrderList> GetTaskOrderDisplayName()
        {
            var taskOrderList = Mapper.Map<IReadOnlyList<TaskOrderDisplayName>, IReadOnlyList<TaskOrderList>>(taskOrderDisplayNames);
            return taskOrderList;
        }

        public IconSetting GetDefaultIconPattern()
        {
            var defaultPattern = iconPatternMaster.Where(p => p.IconId == systemSettings.DefaultIconPattern).Select(p => p).First();
            return Mapper.Map<IconSetting>(defaultPattern);
        }

        public ColorSetting GetDefaultColorPattern()
        {
            var defaultPattern = colorPatternMaster.Where(p => p.ColorId == systemSettings.DefaultColorPattern).Select(p => p).First();
            return Mapper.Map<ColorSetting>(defaultPattern);
        }

        public TodoItem GetDefaultTabSetting(int newTodoId, string newName)
        {
            var defaultTodo = new TodoItem(newTodoId, newName);
            defaultTodo.IsActive.Value = true;
            defaultTodo.UseTristate.Value = systemSettings.DefaultUseTristate;
            defaultTodo.TaskOrder.Value = systemSettings.DefaultTaskOrder.EnumValue<TaskOrderPattern>();
            defaultTodo.IconPattern = GetDefaultIconPattern();
            defaultTodo.ColorPattern = GetDefaultColorPattern();

            return defaultTodo;
        }

        #region SelectTodo Supporter
        private Todo JoinIconMaster(Todo todo, IconSetting icon)
        {
            todo.IconPattern = icon;
            return todo;
        }

        Todo JoinColorMaster(Todo todo, ColorSetting color)
        {
            todo.ColorPattern = color;
            return todo;
        }
        #endregion

        public stt.Task<IEnumerable<TodoItem>> SelectTodoAllAsync()
        {
            var allTodo = primaryRealm.All<Todo>().ToList(); //DB内でJoinは実行できないので
            var todoQuery = allTodo
                             .Join(this.iconPatternMaster, todo => todo.IconPatternId, icon => icon.IconId, (todo, icon) => JoinIconMaster(todo, new IconSetting { CheckedIcon = icon.CheckedIcon, CanceledIcon = icon.CanceledIcon }))
                             .Join(this.colorPatternMaster, todo => todo.ColorPatternId, color => color.ColorId, (todo, color) => JoinColorMaster(todo, Mapper.Map<ColorPatternMaster, ColorSetting>(color)))
                             .OrderBy(todo => todo.DisplayOrder);
            var todoList = todoQuery.ToList();

            return stt.Task.Run(() =>
            {
                var todoAll = Mapper.Map<List<Todo>, IEnumerable<TodoItem>>(todoList);
                return todoAll;
            });
        }

        public stt.Task<IEnumerable<TodoTask>> SelectTaskAllAsync(int todoId)
        {
            var taskQuery = primaryRealm.All<Task>().Where(task => task.TodoId == todoId).OrderBy(task => task.DisplayOrder);
            var taskList = taskQuery.ToList();

            return stt.Task.Run(() =>
            {
                var taskAll = Mapper.Map<List<Task>, IEnumerable<TodoTask>>(taskList);
                return taskAll;
            });
        }
        #endregion

        #region Private
        private void CopyEmbeddedDb(string copyPath)
        {
            var assembly = this.GetType().Assembly;

            using (var db = File.Create(copyPath))
            using (var master = assembly.GetManifestResourceStream("SimpleTodo.Data.master.realm"))
            {
                master.CopyToAsync(db).Wait();
                db.FlushAsync().Wait();
            }
        }

        private RealmConfigurationBase MakeRealmConfiguration(ulong schemaVersion, string systemVersion)
        {
            var config = new RealmConfiguration(dbFile)
            {
                SchemaVersion = schemaVersion,
                ShouldCompactOnLaunch = (totalSize, dataSize) => ((double)(totalSize - dataSize) / dataSize > 1.2),
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    //master.realmを展開

                    //マスターレコードをアップグレード

                    //ユーザーデータの移行

                    //システムバージョンの更新

                    //master.realmの回収
                }
            };
            return config;
        }

        private void SelectCommonMaster()
        {
            systemSettings = primaryRealm.All<SystemSettings>().First();
            lastPage = primaryRealm.All<LastPage>().First();
            menuIconMaster = primaryRealm.All<MenuIconMaster>().First();
            iconPatternMaster = primaryRealm.All<IconPatternMaster>().AsRealmCollection();
            colorPatternMaster = primaryRealm.All<ColorPatternMaster>().AsRealmCollection();
            taskOrderDisplayNames = primaryRealm.All<TaskOrderDisplayName>().AsRealmCollection();
        }
        #endregion

        #region Mapping
        public static void PrepareMapping()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.RecognizePrefixes("inner_");
                cfg.RecognizeDestinationPrefixes("inner_");
                cfg.CreateMap<string, TaskOrderPattern>().ConvertUsing(s => s.EnumValue<TaskOrderPattern>());
                cfg.CreateMap<string, TaskStatus>().ConvertUsing(s => s.EnumValue<TaskStatus>());
                cfg.CreateMap<int, Color>().ConvertUsing(c => Color.FromArgb(c));
                cfg.CreateMap<TaskOrderPattern, string>().ConvertUsing(p => p.EnumName());
                cfg.CreateMap<TaskStatus, string>().ConvertUsing(s => s.EnumName());
                cfg.CreateMap<Todo, TodoItem>();
                cfg.CreateMap<Task, TodoTask>();
                cfg.CreateMap<TodoItem, Todo>();
                cfg.CreateMap<TodoTask, Task>();
                cfg.CreateMap<TaskOrderDisplayName, TaskOrderList>();
                cfg.CreateMap<IconPatternMaster, IconSetting>();
                cfg.CreateMap<ColorPatternMaster, ColorSetting>();
            });
        }
        #endregion
    }

    #region Schema Object
    public class SystemVersion : RealmObject
    {
        public string Version { get; set; }
    }

    public class LastPage : RealmObject
    {
        public string LastViewing { get; set; }
        public int LastFocus { get; set; }
        public int Origin { get; set; }
    }

    public class TodoIdMaster : RealmObject
    {
        public int NextTodoId { get; set; }
    }

    public class TaskIdMaster : RealmObject
    {
        [PrimaryKey]
        public int TodoId { get; set; }
        public int NextTaskId { get; set; }
    }

    public class SystemSettings : RealmObject
    {
        public bool UseBigIcon { get; set; }
        public bool BeginFromTabList { get; set; }
        public string HorizontalMenuBarPosition { get; set; }
        public string NewTabPosition { get; set; }
        public bool DefaultUseTristate { get; set; }
        public string DefaultTaskOrder { get; set; }
        public int DefaultIconPattern { get; set; } //IconPatternMaster
        public int DefaultColorPattern { get; set; } //ColorPatternMaster
    }

    public class MenuIconMaster : RealmObject
    {
        public string TabListIcon { get; set; }
        public string NewTabIcon { get; set; }
        public string NewTaskIcon { get; set; }
        public string Up { get; set; }
        public string Down { get; set; }
        public string TabSetting { get; set; }
        public string SwitchOnOff { get; set; }
    }

    public class IconPatternMaster : RealmObject
    {
        [PrimaryKey]
        public int IconId { get; set; }
        public string CheckedIcon { get; set; }
        public string CanceledIcon { get; set; }
    }

    public class ColorPatternMaster : RealmObject
    {
        //enum System.Drawing.Color

        [PrimaryKey]
        public int ColorId { get; set; }
        public int ColorSelectorDrawing { get; set; } //将来的機能拡張用
        public int PageBasicBackgroundColor { get; set; }   //ContentPage,ContentView
        public int ViewBasicTextColor { get; set; }         //Label,Button,Entry,etc...
        public int NavigationBarBackgroundColor { get; set; }
        public int NavigationBarTextColor { get; set; }
        public int MenuBarBackgroundColor { get; set; }
        public int TabListViewBackgroundColor { get; set; }
        public int TabListViewSeparatorColor { get; set; }
        public int TabListViewCellColor { get; set; }
        public int TabListViewTextColor { get; set; }
        public int TabListViewCellSelectedColor { get; set; }
        public int TabListViewTextSelectedColor { get; set; }
        public int TabBarBackgroundColor { get; set; }
        public int TabBarTextColor { get; set; }
        public int TodoViewBackgroundColor { get; set; }
        public int TodoViewSeparatorColor { get; set; }
        public int TodoViewCellColor { get; set; }
        public int TodoViewTextColor { get; set; }
        public int TodoViewCheckAreaBackgroundColor { get; set; }
        public int TodoViewCellSelectedColor { get; set; }
        public int TodoViewTextSelectedColor { get; set; }
        public int SlideMenuBackgroundColor { get; set; }
        public int SlideMenuCellBackgroundColor { get; set; }
        public int SlideMenuCellTextColor { get; set; }
        public int SlideMenuPickerBackgroundColor { get; set; }
        public int SlideMenuPickerTextColor { get; set; }
        public int SwitchOnColor { get; set; }             //Switch,SwitchCell
        public int SwitchTintColor { get; set; }
        public int SwitchThumbColor { get; set; }
    }

    public class TaskOrderDisplayName : RealmObject
    {
        [PrimaryKey]
        public string TaskOrder { get; set; }
        public string DisplayName { get; set; }
    }

    public class Todo : RealmObject
    {
        [PrimaryKey]
        public int TodoId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool UseTristate { get; set; }
        public string TaskOrder { get; set; }
        public int IconPatternId { get; set; } //key IconPatternMaster
        public int ColorPatternId { get; set; } //key ColorPatternMaster
        public bool IndependentSetting { get; set; }

        [Ignored]
        internal IconSetting IconPattern { get; set; }
        [Ignored]
        internal ColorSetting ColorPattern { get; set; }
    }

    public class Task : RealmObject
    {
        [PrimaryKey]
        public long TodoTaskId { get; set; }
        [Indexed]
        public int TodoId { get; set; }
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int DisplayOrder { get; set; }
    }
    #endregion
}
