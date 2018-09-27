using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using stt = System.Threading.Tasks;

using PCLStorage;
using Realms;
using AutoMapper;
using RealmDb = Realms.Realm;

using EventRouting;

namespace SimpleTodo.Realm
{
    public class RealmAccess : IDataAccess
    {
        private RealmConfigurationBase commonConfig;
        private volatile RealmDb realm;

        private string dbFile;

        private object _lock_ = new object();

        private ReactiveSequencer<Action<RealmDb>> asyncSequencer;

        #region Master Table
        private volatile SystemSettings systemSettings;
        private volatile MenuIconMaster menuIconMaster;
        private volatile IRealmCollection<IconPatternMaster> iconPatternMaster;
        private volatile IRealmCollection<ColorPatternMaster> colorPatternMaster;
        private volatile IRealmCollection<TaskOrderDisplayName> taskOrderDisplayNames;
        #endregion

        #region Stored Information
        private volatile LastPage lastPage;

        private ThreadSafeReference.Object<LastPage> threadSafeLastPage;
        #endregion

        #region Connect
        public RealmAccess()
        {
            InitializeRealm();
            OpenConnection();

            var processor = new AsyncRealmProcessor(commonConfig);
            processor.ExceptionOccured.Subscribe(ThrowDataAccessException);
            asyncSequencer = new ReactiveSequencer<Action<RealmDb>>(processor);
        }

        public void CloseConnection()
        {
            if (realm != null) realm.Dispose();
        }

        private void InitializeRealm()
        {
            const int CurrentSchemaVertion = 0; //これを間違うと死ぬ！

            var folder = FileSystem.Current.LocalStorage;
            dbFile = Path.Combine(folder.Path, "item.realm");
            commonConfig = MakeRealmConfiguration(CurrentSchemaVertion, this.GetType().Assembly.GetName().Version.ToString());
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

            realm = RealmDb.GetInstance(commonConfig);

            SelectCommonMaster();
        }
        #endregion

        #region Get/Set Value
        public string GetSystemVersion()
        {
            return realm.All<SystemVersion>().FirstOrDefault().Version;
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
        public void SetDefaultUseTristateAsync(bool value) => SettleAsyncTransaction(systemSettings, (db, s) => s.DefaultUseTristate = value);

        public TaskOrderPattern GetDefaultTaskOrder() => systemSettings.DefaultTaskOrder.EnumValue<TaskOrderPattern>();
        public void SetDefaultTaskOrderAsync(TaskOrderPattern order) => SettleAsyncTransaction(systemSettings, (db, s) => s.DefaultTaskOrder = order.EnumName());

        public bool IsBigIcon() => systemSettings.UseBigIcon;
        public void UseBigIconAsync(bool usage) => SettleAsyncTransaction(systemSettings, (db, s) => s.UseBigIcon = usage);

        public bool IsBeginFromTabList() => systemSettings.BeginFromTabList;
        public void BeginFromTabListAsync(bool begin) => SettleAsyncTransaction(systemSettings, (db, s) => s.BeginFromTabList = begin);

        public MenuBarPosition GetMenuBarPosition() => systemSettings.HorizontalMenuBarPosition.EnumValue<MenuBarPosition>();
        public void SetMenuBarPositionAsync(MenuBarPosition position) => SettleAsyncTransaction(systemSettings, (db, s) => s.HorizontalMenuBarPosition = position.EnumName());

        public TabPosition GetNewTabPosition() => systemSettings.NewTabPosition.EnumValue<TabPosition>();
        public void SetNewTabPositionAsync(TabPosition position) => SettleAsyncTransaction(systemSettings, (db, s) => s.NewTabPosition = position.EnumName());

        public ViewingPage GetLastPage() => lastPage.LastViewing.EnumValue<ViewingPage>();
        public void SetLastPageAsync(ViewingPage viewing) => SettleAsyncTransaction(lastPage, (db, p) => p.LastViewing = viewing.EnumName());

        public int GetLastTabIndex() => lastPage.LastFocus;
        public void SetLastTabIndexAsync(int focus) => SettleAsyncTransaction(lastPage, (db, p) => p.LastFocus = focus);

        public int GetOriginTabIndex() => lastPage.Origin;
        public void SetOriginTabIndexAsync(int origin) => SettleAsyncTransaction(lastPage, (db, p) => p.Origin = origin);

        public IReadOnlyList<TaskOrderList> GetTaskOrderList()
            => taskOrderDisplayNames.OrderBy(o => (int)o.TaskOrder.EnumValue<TaskOrderPattern>()).Select(o => Mapper.Map<TaskOrderList>(o)).ToList();

        public int GetNewTodoId()
        {
            //これは非同期にできない

            int newId = 0;
            var next = realm.All<TodoIdMaster>().FirstOrDefault();
            if (next != null)
            {
                newId = next.NextTodoId;
            }

            SettleAsyncTransaction(next, (db, m) =>
            {
                if (m != null) m.NextTodoId = newId + 1;
                else db.Add(new TodoIdMaster { NextTodoId = 1 });
            });

            return newId;
        }

        public int GetNewTaskId(int todoId)
        {
            //これは非同期にできない

            int newId = 0;
            var next = realm.All<TaskIdMaster>().Where(m => m.TodoId == todoId).FirstOrDefault();
            if (next != null)
            {
                newId = next.NextTaskId;
            }

            SettleAsyncTransaction(next, (db, m) =>
            {
                if (m != null) m.NextTaskId = newId + 1;
                else db.Add(new TaskIdMaster { TodoId = todoId, NextTaskId = 1 });
            });

            return newId;
        }
        #endregion

        #region Todo Task
        public TodoItem AddTodoAsync(TodoItem todoItem)
        {
            SettleAsyncTransaction((RealmDb db, Todo n) =>
            {
                var todo = Mapper.Map<Todo>(todoItem);
                db.Add(todo);
            });
            return todoItem;
        }

        public TodoTask AddTaskAsync(int todoId, TodoTask todoTask)
        {
            SettleAsyncTransaction((RealmDb db, Task n) =>
            {
                var task = Mapper.Map<Task>(todoTask);
                task.TodoId = todoId;
                db.Add(task);
            });
            return todoTask;
        }

        public void ChangeVisibilityAsync(int todoId, bool visibile)
        {
            SettleAsyncTransaction((RealmDb db, Todo n) =>
            {
                var todo = db.All<Todo>().Where(t => t.TodoId == todoId).First();
                db.Write(() => todo.IsActive = visibile);
            });
        }

        public void RenameTodoAsync(int todoId, string newName)
        {
            SettleAsyncTransaction((RealmDb db, Todo n) =>
            {
                var todo = db.All<Todo>().Where(t => t.TodoId == todoId).First();
                db.Write(() => todo.Name = newName);
            });
        }

        public void RenameTaskAsync(int todoId, int taskId, string newName)
        {
            SettleAsyncTransaction((RealmDb db, Task n) =>
            {
                var task = db.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == taskId).First();
                db.Write(() => task.Name = newName);
            });
        }

        public void ReorderTodoAsync(IEnumerable<TodoItem> todoItems)
        {
            SettleAsyncTransaction((RealmDb db, Todo n) =>
            {
                var allTodo = db.All<Todo>();
                int index = 0;
                db.Write(() =>
                {
                    foreach (var todoItem in todoItems)
                    {
                        var todo = allTodo.Where(t => t.TodoId == todoItem.TodoId.Value).First();
                        todo.DisplayOrder = index;
                        index++;
                    }
                });
            });
        }

        public void ReorderTaskAsync(int todoId, IEnumerable<TodoTask> todoTasks)
        {
            SettleAsyncTransaction((RealmDb db, Task n) =>
            {
                var allTask = db.All<Task>().Where(t => t.TodoId == todoId);
                int index = 0;
                db.Write(() =>
                {
                    foreach (var todoTask in todoTasks)
                    {
                        var task = allTask.Where(t => t.TaskId == todoTask.TaskId.Value).First();
                        task.DisplayOrder = index;
                        index++;
                    }
                });
            });
        }

        public void ToggleTaskStatusAsync(int todoId, int taskId, TaskStatus status)
        {
            SettleAsyncTransaction((RealmDb db, Todo n) =>
            {
                var task = db.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == taskId).First();
                db.Write(() => task.Status = status.EnumName());
            });
        }

        public void UpdateTodoAsync(TodoItem todoItem)
        {
            SettleAsyncTransaction((RealmDb db, Todo n) =>
            {
                var todo = db.All<Todo>().Where(t => t.TodoId == todoItem.TodoId.Value).First();
                db.Write(() => Mapper.Map(todoItem, todo));
            });
        }

        public void UpdateTaskAsync(int todoId, TodoTask todoTask)
        {
            SettleAsyncTransaction((RealmDb db, Task n) =>
            {
                var task = db.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == todoTask.TaskId.Value).First();
                db.Write(() => Mapper.Map(todoTask, task));
            });
        }

        public void DeleteTodoAsync(int todoId)
        {
            SettleAsyncTransaction((RealmDb db, Todo n) =>
            {
                var todo = db.All<Todo>().Where(t => t.TodoId == todoId).First();
                db.Write(() => db.Remove(todo));
            });
        }

        public void DeleteTaskAsync(int todoId, int taskId)
        {
            SettleAsyncTransaction((RealmDb db, Task n) =>
            {
                var task = db.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == taskId).First();
                db.Write(() => db.Remove(task));
            });
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
            return stt.Task.Run(() =>
            {
                var asyncRealm = RealmDb.GetInstance(commonConfig);
                var allTodo = asyncRealm.All<Todo>().ToList(); //DB内でJoinは実行できないので
                var todoQuery = allTodo
                                 .Join(this.iconPatternMaster, todo => todo.IconPatternId, icon => icon.IconId, (todo, icon) => JoinIconMaster(todo, new IconSetting { CheckedIcon = icon.CheckedIcon, CanceledIcon = icon.CanceledIcon }))
                                 .Join(this.colorPatternMaster, todo => todo.ColorPatternId, color => color.ColorId, (todo, color) => JoinColorMaster(todo, Mapper.Map<ColorPatternMaster, ColorSetting>(color)))
                                 .OrderBy(todo => todo.DisplayOrder);
                var todoList = todoQuery.ToList();

                var todoAll = Mapper.Map<List<Todo>, IEnumerable<TodoItem>>(todoList);
                return todoAll;
            });
        }

        public stt.Task<IEnumerable<TodoTask>> SelectTaskAllAsync(int todoId)
        {
            return stt.Task.Run(() =>
            {
                var asyncRealm = RealmDb.GetInstance(commonConfig);
                var taskQuery = asyncRealm.All<Task>().Where(task => task.TodoId == todoId).OrderBy(task => task.DisplayOrder);
                var taskList = taskQuery.ToList();

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
            void MigrateMasterRecord<T>(RealmDb masterDb, RealmDb newDb) where T : RealmObject
            {
                var records = masterDb.All<T>();
                foreach (var record in records)
                {
                    newDb.Add(record);
                }
            }

            var config = new RealmConfiguration(dbFile)
            {
                SchemaVersion = schemaVersion,
                ShouldCompactOnLaunch = (totalSize, dataSize) => ((double)(totalSize - dataSize) / dataSize > 1.2),
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    var newRealm = migration.NewRealm;

                    //master.realmを展開
                    var folder = FileSystem.Current.LocalStorage;
                    var tempMasterPath = Path.Combine(folder.Path, "master.realm");
                    CopyEmbeddedDb(tempMasterPath);
                    var masterRealm = RealmDb.GetInstance(new RealmConfiguration(tempMasterPath));

                    //**************モデルが古ければ更新*****************

                    //マスターレコードをアップグレード
                    newRealm.RemoveAll<MenuIconMaster>();
                    newRealm.RemoveAll<IconPatternMaster>();
                    newRealm.RemoveAll<ColorPatternMaster>();
                    newRealm.RemoveAll<TaskOrderDisplayName>();

                    MigrateMasterRecord<MenuIconMaster>(masterRealm, newRealm);
                    MigrateMasterRecord<IconPatternMaster>(masterRealm, newRealm);
                    MigrateMasterRecord<ColorPatternMaster>(masterRealm, newRealm);
                    MigrateMasterRecord<TaskOrderDisplayName>(masterRealm, newRealm);

                    //**************ユーザーデータの移行*****************

                    //システムバージョンの更新
                    var version = newRealm.All<SystemVersion>().First();
                    version.Version = systemVersion;

                    //master.realmの回収
                    masterRealm.Dispose();
                    File.Delete(tempMasterPath);
                }
            };
            return config;
        }

        private void SelectCommonMaster()
        {
            systemSettings = realm.All<SystemSettings>().First();
            lastPage = realm.All<LastPage>().First();
            menuIconMaster = realm.All<MenuIconMaster>().First();

            var iconPatterns = realm.All<IconPatternMaster>();
            var colorPatterns = realm.All<ColorPatternMaster>();
            var taskOrder = realm.All<TaskOrderDisplayName>();
            iconPatternMaster = iconPatterns.AsRealmCollection();
            colorPatternMaster = colorPatterns.AsRealmCollection();
            taskOrderDisplayNames = taskOrder.AsRealmCollection();
        }

        private T Resolve<T>(RealmDb asyncRealm, ThreadSafeReference.Object<T> realmObj) where T : RealmObject => asyncRealm.ResolveReference(realmObj);
        private IQueryable<T> Resolve<T>(RealmDb asyncRealm, ThreadSafeReference.Query<T> realmObj) where T : RealmObject => asyncRealm.ResolveReference(realmObj);
        private IList<T> Resolve<T>(RealmDb asyncRealm, ThreadSafeReference.List<T> realmObj) where T : RealmObject => asyncRealm.ResolveReference(realmObj);

        private void SettleAsyncTransaction<T>(Action<RealmDb, T> transaction) where T : RealmObject => SettleAsyncTransaction(NullRef<T>(), transaction);
        private void SettleAsyncTransaction<T>(T unsafeRealmObject, Action<RealmDb, T> transaction) where T : RealmObject
        {
            var threadSafeRealmObject = ThreadSafeReference.Create(unsafeRealmObject);
            asyncSequencer.Enqueue(asyncRealm =>
            {
                var asyncObj = Resolve(asyncRealm, threadSafeRealmObject);
                asyncRealm.Write(() => transaction(asyncRealm, asyncObj));
            });
        }

        private void SettleAsyncTransaction<T>(Action<RealmDb, IQueryable<T>> transaction) where T : RealmObject => SettleAsyncTransaction(NullRefQ<T>(), transaction);
        private void SettleAsyncTransaction<T>(IQueryable<T> unsafeRealmObject, Action<RealmDb, IQueryable<T>> transaction) where T : RealmObject
        {
            var threadSafeRealmObject = ThreadSafeReference.Create(unsafeRealmObject);
            asyncSequencer.Enqueue(asyncRealm =>
            {
                var asyncObj = Resolve(asyncRealm, threadSafeRealmObject);
                asyncRealm.Write(() => transaction(asyncRealm, asyncObj));
            });
        }

        private void SettleAsyncTransaction<T>(Action<RealmDb, IList<T>> transaction) where T : RealmObject => SettleAsyncTransaction(NullRefL<T>(), transaction);
        private void SettleAsyncTransaction<T>(IList<T> unsafeRealmObject, Action<RealmDb, IList<T>> transaction) where T : RealmObject
        {
            var threadSafeRealmObject = ThreadSafeReference.Create(unsafeRealmObject);
            asyncSequencer.Enqueue(asyncRealm =>
            {
                var asyncObj = Resolve(asyncRealm, threadSafeRealmObject);
                asyncRealm.Write(() => transaction(asyncRealm, asyncObj));
            });
        }

        private T NullRef<T>() where T : RealmObject => null;
        private IQueryable<T> NullRefQ<T>() where T : RealmObject => null;
        private IList<T> NullRefL<T>() where T : RealmObject => null;
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

        private void ThrowDataAccessException(ReactiveProcessorException ex)
        {
            //TODO: 処理内容に応じた対処が必要？

            var capsule = new DataAccessException(ex.InnerException, ex.CausedThreadId);
            throw capsule;
        }

        private class AsyncRealmProcessor : ReactiveProcessorBase<Action<RealmDb>>
        {
            private RealmConfigurationBase commonConfig;
            internal AsyncRealmProcessor(RealmConfigurationBase config) => this.commonConfig = config;

            protected override void Execute(Action<RealmDb> transaction)
            {
                var asyncRealm = RealmDb.GetInstance(commonConfig);
                transaction(asyncRealm);
            }
        }
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
