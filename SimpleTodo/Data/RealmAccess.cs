using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using stt = System.Threading.Tasks;

using PCLStorage;
using Realms;
using Reactive.Bindings;
using AutoMapper;

namespace SimpleTodo
{
    public class RealmAccess
    {
        private Realm realm;
        private Transaction transaction;

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
            var folder = FileSystem.Current.LocalStorage;
            dbFile = Path.Combine(folder.Path, "item.realm");
            OpenConnectionAsync().Wait();
        }

        public void CloseConnection()
        {
            realm.Dispose();
            realm = null;
        }

        public async stt.Task OpenConnectionAsync()
        {
            const int CurrentSchemaVertion = 0; //これを間違うと死ぬ！

            if (!File.Exists(dbFile))
            {
                await CopyEmbeddedDb(dbFile);
            }
            realm = await Realm.GetInstanceAsync(MakeRealmConfiguration(CurrentSchemaVertion, this.GetType().Assembly.GetName().Version.ToString()));
            await SelectCommonMaster();
        }
        #endregion

        #region Get/Set Value
        public string GetSystemVersion()
        {
            return realm.All<SystemVersion>().First().Version;
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
        public async stt.Task SetDefaultUseTristateAsync(bool value) => await realm.WriteAsync((realmAsync) => systemSettings.DefaultUseTristate = value);

        public TaskOrderPattern GetDefaultTaskOrder() => systemSettings.DefaultTaskOrderEnum;
        public async stt.Task SetDefaultTaskOrderAsync(TaskOrderPattern order) => await realm.WriteAsync((realmAsync) => systemSettings.DefaultTaskOrder = EnumName(order));

        public bool IsBigIcon() => systemSettings.UseBigIcon;
        public async stt.Task UseBigIconAsync(bool usage) => await realm.WriteAsync((realmAsync) => systemSettings.UseBigIcon = usage);

        public bool IsBeginFromTabList() => systemSettings.BeginFromTabList;
        public async stt.Task BeginFromTabListAsync(bool begin) => await realm.WriteAsync((realmAsync) => systemSettings.BeginFromTabList = begin);

        public MenuBarPosition GetMenuBarPosition() => systemSettings.HorizontalMenuBarPositionEnum;
        public async stt.Task SetMenuBarPositionAsync(MenuBarPosition position) => await realm.WriteAsync((realmAsync) => systemSettings.HorizontalMenuBarPositionEnum = position);

        public TabPosition GetNewTabPosition() => systemSettings.NewTabPositionEnum;
        public async stt.Task SetNewTabPositionAsync(TabPosition position) => await realm.WriteAsync((realmAsync) => systemSettings.NewTabPositionEnum = position);

        public ViewingPage GetLastPage() => lastPage.LastViewingEnum;
        public async stt.Task SetLastPageAsync(ViewingPage viewing) => await realm.WriteAsync((realmAsync) => lastPage.LastViewingEnum = viewing);

        public int GetLastTabIndex() => lastPage.LastFocus;
        public async stt.Task SetLastTabIndexAsync(int focus) => await realm.WriteAsync((realmAsync) => lastPage.LastFocus = focus);

        public int GetOriginTabIndex() => lastPage.Origin;
        public async stt.Task SetOriginTabIndexAsync(int origin) => await realm.WriteAsync((realmAsync) => lastPage.Origin = origin);

        public int GetNewTodoId()
        {
            //これは非同期にできない

            int newId = 0;
            using (var transaction = realm.BeginWrite())
            {
                var next = realm.All<TodoIdMaster>().FirstOrDefault();
                if (next != null)
                {
                    newId = next.NextTodoId;
                    next.NextTodoId = newId++;
                }
                else
                {
                    realm.Add(new TodoIdMaster { NextTodoId = newId++ });
                }
                transaction.Commit();
            }
            return newId;
        }

        public int GetNewTaskId(int todoId)
        {
            //これは非同期にできない

            int newId = 0;
            using (var transaction = realm.BeginWrite())
            {
                var next = realm.All<TaskIdMaster>().Where(m => m.TodoId == todoId).FirstOrDefault();
                if (next != null)
                {
                    newId = next.NextTaskId;
                    next.NextTaskId = newId++;
                }
                else
                {
                    realm.Add(new TaskIdMaster { TodoId = todoId, NextTaskId = newId++ });
                }
                transaction.Commit();
            }
            return newId;
        }
        #endregion

        #region DML
        public void BeginTransaction() => transaction = realm.BeginWrite();
        public void Rollback() { transaction?.Rollback(); transaction = null; }
        public void Commit() { transaction?.Commit(); transaction = null; }
        public void Write(Action action) { transaction?.Rollback(); transaction = null; realm.Write(action); }
        public async stt.Task WriteAsync(Action<Realm> action) { transaction?.Rollback(); transaction = null; await realm.WriteAsync(action); }

        public stt.Task<TodoItem> AddTodoAsync(TodoItem todoItem)
        {
            return stt.Task<TodoItem>.Run(async () =>
            {
                var todo = Mapper.Map<Todo>(todoItem);
                await realm.WriteAsync((realmAsync) => realmAsync.Add(todo));
                return todoItem;
            });
        }

        public stt.Task<TodoTask> AddTaskAsync(int todoId, TodoTask todoTask)
        {
            return stt.Task<TodoTask>.Run(async () =>
            {
                var task = Mapper.Map<Task>(todoTask);
                task.TodoId = todoId;
                await realm.WriteAsync((realmAsync) => realmAsync.Add(task));
                return todoTask;
            });
        }

        public stt.Task RenameTodoAsync(int todoId, string newName)
        {
            return stt.Task.Run(async () =>
            {
                var todo = realm.All<Todo>().Where(t => t.TodoId == todoId).First();
                await realm.WriteAsync((realmAsync) => todo.Name = newName);
            });
        }

        public stt.Task RenameTaskAsync(int todoId, int taskId, string newName)
        {
            return stt.Task.Run(async () =>
            {
                var task = realm.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == taskId).First();
                await realm.WriteAsync((realmAsync) => task.Name = newName);
            });
        }

        public stt.Task UpdateTodoAsync(TodoItem todoItem)
        {
            return stt.Task.Run(async () =>
            {
                var todo = realm.All<Todo>().Where(t => t.TodoId == todoItem.TodoId.Value).First();
                await realm.WriteAsync((realmAsync) => Mapper.Map(todoItem, todo));
            });
        }

        public stt.Task UpdateTaskAsync(int todoId, TodoTask todoTask)
        {
            return stt.Task.Run(async () =>
            {
                var task = realm.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == todoTask.TaskId.Value).First();
                await realm.WriteAsync((realmAsync) => Mapper.Map(todoTask, task));
            });
        }

        public stt.Task DeleteTodoAsync(int todoId)
        {
            return stt.Task.Run(async () =>
            {
                var todo = realm.All<Todo>().Where(t => t.TodoId == todoId).First();
                await realm.WriteAsync((realmAsync) => realmAsync.Remove(todo));
            });
        }

        public stt.Task DeleteTaskAsync(int todoId, int taskId)
        {
            return stt.Task.Run(async () =>
            {
                var task = realm.All<Task>().Where(t => t.TodoId == todoId && t.TaskId == taskId).First();
                await realm.WriteAsync((realmAsync) => realmAsync.Remove(task));
            });
        }

        #region Generic Interface
        public T Insert<T>(T record) where T : RealmObject => realm.Add<T>(record);
        public IList<T> Insert<T>(IEnumerable<T> records) where T : RealmObject
        {
            List<T> result = new List<T>();
            foreach (var record in records)
            {
                result.Add(realm.Add<T>(record));
            }
            return result;
        }
        public T InsertAuto<T>(T record) where T : RealmObject
        {
            realm.Write(() => realm.Add<T>(record));
            return record;
        }
        public stt.Task<T> InsertAutoAsync<T>(T record) where T : RealmObject
        {
            return stt.Task<T>.Run(async () =>
            {
                await realm.WriteAsync((realmAsync) => realmAsync.Add<T>(record));
                return record;
            });
        }
        public IList<T> InsertAuto<T>(IEnumerable<T> records) where T : RealmObject
        {
            List<T> result = new List<T>();
            realm.Write(() =>
            {
                foreach (var record in records)
                {
                    result.Add(realm.Add<T>(record));
                }
            });
            return result;
        }
        public stt.Task<IList<T>> InsertAutoAsync<T>(IEnumerable<T> records) where T : RealmObject
        {
            return stt.Task<IList<T>>.Run(async () =>
            {
                List<T> result = new List<T>();
                await realm.WriteAsync((realmAsync) =>
                {
                    foreach (var record in records)
                    {
                        result.Add(realmAsync.Add<T>(record));
                    }
                });
                return (IList<T>)result;
            });
        }
        public T Update<T>(T record) where T : RealmObject => realm.Add<T>(record, false); //PrimaryKeyがないとupdate:trueにできない
        public IList<T> Update<T>(IEnumerable<T> records) where T : RealmObject
        {
            List<T> result = new List<T>();
            foreach (var record in records)
            {
                result.Add(realm.Add<T>(record, false));
            }
            return result;
        }
        public T UpdateAuto<T>(T record, Action<T> mapper) where T : RealmObject
        {
            realm.Write(() =>
            {
                mapper(record);
            });
            return record;
        }
        public void UpdateAuto<T>(T record, UpdateMapper<T> mappers) where T : RealmObject
        {
            realm.Write(() =>
            {
                foreach (var mapper in mappers.MapList)
                    mapper(record);
            });
        }
        public stt.Task<T> UpdateAutoAsync<T>(T record, Action<T> mapper) where T : RealmObject
        {
            return stt.Task<T>.Run(async () =>
            {
                await realm.WriteAsync((realmAcync) =>
                {
                    mapper(record);
                });
                return record;
            });
        }
        public stt.Task UpdateAutoAsync<T>(T record, UpdateMapper<T> mappers) where T : RealmObject
        {
            return stt.Task.Run(async () =>
            {
                await realm.WriteAsync((realmAsync) =>
                {
                    foreach (var mapper in mappers.MapList)
                        mapper(record);
                });
            });
        }
        public void Delete<T>(T record) where T : RealmObject => realm.Remove(record);
        public void DeleteAuto<T>(T record) where T : RealmObject
        {
            realm.Write(() =>
            {
                realm.Remove(record);
            });
        }
        public stt.Task DeleteAutoAsync<T>(T record) where T : RealmObject
        {
            return stt.Task.Run(async () =>
            {
                await realm.WriteAsync((realmAsync) =>
                {
                    realmAsync.Remove(record);
                });
            });
        }
        public void Truncate<T>() where T : RealmObject => realm.RemoveAll<T>();
        public void TruncateAuto<T>() where T : RealmObject
        {
            realm.Write(() =>
            {
                realm.RemoveAll<T>();
            });
        }
        public stt.Task TruncateAutoAsync<T>() where T : RealmObject
        {
            return stt.Task.Run(async () =>
            {
                await realm.WriteAsync((realmAsync) =>
                {
                    realmAsync.RemoveAll<T>();
                });
            });
        }
        #endregion

        #endregion

        #region Query
        public stt.Task<IReadOnlyList<IconSetting>> GetIconPatternAllAsync()
        {
            return stt.Task<IReadOnlyList<IconSetting>>.Run(() =>
            {
                return Mapper.Map<IRealmCollection<IconPatternMaster>, IReadOnlyList<IconSetting>>(iconPatternMaster);
            });
        }

        public stt.Task<IReadOnlyList<ColorSetting>> GetColorPatternAllAsync()
        {
            return stt.Task<IReadOnlyList<ColorSetting>>.Run(() =>
            {
                return Mapper.Map<IRealmCollection<ColorPatternMaster>, IReadOnlyList<ColorSetting>>(colorPatternMaster);
            });
        }

        public IList<TaskOrderDisplayName> GetTaskOrderDisplayName()
        {
            return taskOrderDisplayNames.ToList();
        }

        public stt.Task<IconSetting> GetDefaultIconPatternAsync()
        {
            return stt.Task.Run(() =>
            {
                var defaultPattern = iconPatternMaster.Where(p => p.IconId == systemSettings.DefaultIconPattern).Select(p => p).First();
                return Mapper.Map<IconSetting>(defaultPattern);
            });
        }

        public stt.Task<ColorSetting> GetDefaultColorPatternAsync()
        {
            return stt.Task.Run(() =>
            {
                var defaultPattern = colorPatternMaster.Where(p => p.ColorId == systemSettings.DefaultColorPattern).Select(p => p).First();
                return Mapper.Map<ColorSetting>(defaultPattern);
            });
        }

        public stt.Task<TodoItem> GetDefaultTabSettingAsync(int newTodoId, string newName)
        {
            return stt.Task<TodoItem>.Run(() =>
            {
                var defaultTodo = new TodoItem(newTodoId, newName);
                defaultTodo.IsActive.Value = true;
                defaultTodo.UseTristate.Value = systemSettings.DefaultUseTristate;
                defaultTodo.TaskOrder.Value = systemSettings.DefaultTaskOrderEnum;
                defaultTodo.IconPattern = GetDefaultIconPatternAsync().Result;
                defaultTodo.ColorPattern = GetDefaultColorPatternAsync().Result;

                return defaultTodo;
            });
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
                var todoQuery = realm.All<Todo>().OrderBy(todo => todo.DisplayOrder)
                                     .Join(this.iconPatternMaster, todo => todo.IconPatternId, icon => icon.IconId, (todo, icon) => JoinIconMaster(todo, new IconSetting { CheckedIcon = icon.CheckedIcon, CanceledIcon = icon.CanceledIcon }))
                                     .Join(this.colorPatternMaster, todo => todo.ColorPatternId, color => color.ColorId, (todo, color) => JoinColorMaster(todo, Mapper.Map<ColorPatternMaster, ColorSetting>(color)))
                                     .Select(result => result);
                var todoList = todoQuery.ToList();
                var todoAll = Mapper.Map<List<Todo>, IEnumerable<TodoItem>>(todoList);
                return todoAll;
            });
        }

        public stt.Task<IEnumerable<TodoTask>> SelectTaskAllAsync(int todoId)
        {
            return stt.Task.Run(() =>
            {
                var taskQuery = realm.All<Task>().Where(task => task.TodoId == todoId).OrderBy(task => task.DisplayOrder);
                var taskList = taskQuery.ToList();
                var taskAll = Mapper.Map<List<Task>, IEnumerable<TodoTask>>(taskList);
                return taskAll;
            });
        }
        #endregion

        #region Private
        private async stt.Task CopyEmbeddedDb(string copyPath)
        {
            var assembly = this.GetType().Assembly;

            using (var db = File.Create(dbFile))
            using (var master = assembly.GetManifestResourceStream("SimpleTodo.Data.master.realm"))
            {
                await master.CopyToAsync(db);
                await db.FlushAsync();
            }
        }

        private RealmConfigurationBase MakeRealmConfiguration(ulong schemaVersion, string systemVersion)
        {
            var config = new RealmConfiguration(dbFile)
            {
                SchemaVersion = schemaVersion,
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

        private stt.Task SelectCommonMaster()
        {
            return stt.Task.Run(() =>
            {
                systemSettings = realm.All<SystemSettings>().First();
                lastPage = realm.All<LastPage>().First();
                menuIconMaster = realm.All<MenuIconMaster>().First();
                iconPatternMaster = realm.All<IconPatternMaster>().AsRealmCollection();
                colorPatternMaster = realm.All<ColorPatternMaster>().AsRealmCollection();
                taskOrderDisplayNames = realm.All<TaskOrderDisplayName>().AsRealmCollection();
            });
        }

        private string EnumName<TEnum>(TEnum value) => Enum.GetName(typeof(TEnum), value);
        #endregion

        #region Mapping
        public static void PrepareMapping()
        {
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.RecognizePrefixes("inner_");
                cfg.RecognizeDestinationPrefixes("inner_");
                cfg.CreateMap<string, TaskOrderPattern>().ConvertUsing(s => (TaskOrderPattern)Enum.Parse(typeof(TaskOrderPattern), s));
                cfg.CreateMap<string, TaskStatus>().ConvertUsing(s => (TaskStatus)Enum.Parse(typeof(TaskStatus), s));
                cfg.CreateMap<string, Color>().ConvertUsing(s => Color.FromName(s));
                cfg.CreateMap<TaskOrderPattern, string>().ConvertUsing(p => Enum.GetName(typeof(TaskOrderPattern), p));
                cfg.CreateMap<TaskStatus, string>().ConvertUsing(s => Enum.GetName(typeof(TaskStatus), s));
                cfg.CreateMap<Todo, TodoItem>();
                cfg.CreateMap<Task, TodoTask>();
                cfg.CreateMap<TodoItem, Todo>();
                cfg.CreateMap<TodoTask, Task>();
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
        public string LastViewing
        {
            get => Enum.GetName(typeof(ViewingPage), LastViewingEnum);
            set => LastViewingEnum = (ViewingPage)Enum.Parse(typeof(ViewingPage), value);
        }
        public int LastFocus { get; set; }
        public int Origin { get; set; }

        [Ignored]
        [IgnoreMap]
        public ViewingPage LastViewingEnum { get; set; }
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
        public string HorizontalMenuBarPosition
        {
            get => Enum.GetName(typeof(MenuBarPosition), HorizontalMenuBarPositionEnum);
            set => HorizontalMenuBarPositionEnum = (MenuBarPosition)Enum.Parse(typeof(MenuBarPosition), value);
        }
        public string NewTabPosition
        {
            get => Enum.GetName(typeof(TabPosition), NewTabPositionEnum);
            set => NewTabPositionEnum = (TabPosition)Enum.Parse(typeof(TabPosition), value);
        }
        public bool DefaultUseTristate { get; set; }
        public string DefaultTaskOrder
        {
            get => Enum.GetName(typeof(TaskOrderPattern), DefaultTaskOrderEnum);
            set => DefaultTaskOrderEnum = (TaskOrderPattern)Enum.Parse(typeof(TaskOrderPattern), value);
        }
        public int DefaultIconPattern { get; set; } //IconPatternMaster
        public int DefaultColorPattern { get; set; } //ColorPatternMaster

        [Ignored]
        [IgnoreMap]
        public MenuBarPosition HorizontalMenuBarPositionEnum { get; set; }
        [Ignored]
        [IgnoreMap]
        public TabPosition NewTabPositionEnum { get; set; }
        [Ignored]
        [IgnoreMap]
        public TaskOrderPattern DefaultTaskOrderEnum { get; set; }
    }

    public class MenuIconMaster : RealmObject
    {
        public string TabListIcon { get; set; }
        public string NewTaskIcon { get; set; }
        public string Up { get; set; }
        public string Down { get; set; }
        public string TabSetting { get; set; }
        public string NewTabIcon { get; set; }
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
        public string PageBasicBackgroundColor { get; set; }   //ContentPage,ContentView
        public string ViewBasicTextColor { get; set; }         //Label,Button,Entry,etc...
        public string NavigationBarBackgroundColor { get; set; }
        public string NavigationBarTextColor { get; set; }
        public string MenuBarBackgroundColor { get; set; }
        public string TabListViewBackgroundColor { get; set; }
        public string TabListViewSeparatorColor { get; set; }
        public string TabListViewCellColor { get; set; }
        public string TabListViewTextColor { get; set; }
        public string TabListViewCellSelectedColor { get; set; }
        public string TabListViewTextSelectedColor { get; set; }
        public string TabBarBackgroundColor { get; set; }
        public string TabBarTextColor { get; set; }
        public string TodoViewBackgroundColor { get; set; }
        public string TodoViewSeparatorColor { get; set; }
        public string TodoViewCellColor { get; set; }
        public string TodoViewTextColor { get; set; }
        public string TodoViewCheckAreaBackgroundColor { get; set; }
        public string TodoViewCellSelectedColor { get; set; }
        public string TodoViewTextSelectedColor { get; set; }
        public string SlideMenuBackgroundColor { get; set; }
        public string SlideMenuCellBackgroundColor { get; set; }
        public string SlideMenuCellTextColor { get; set; }
        public string SlideMenuPickerBackgroundColor { get; set; }
        public string SlideMenuPickerTextColor { get; set; }
        public string SwitchOnColor { get; set; }             //Switch,SwitchCell
    }

    public class TaskOrderDisplayName : RealmObject
    {
        public string TaskOrder
        {
            get => Enum.GetName(typeof(TaskOrderPattern), TaskOrderEnum);
            set => TaskOrderEnum = (TaskOrderPattern)Enum.Parse(typeof(TaskOrderPattern), value);
        }
        public string DisplayName { get; set; }

        [Ignored]
        [IgnoreMap]
        public TaskOrderPattern TaskOrderEnum { get; set; }
    }

    public class Todo : RealmObject
    {
        [PrimaryKey]
        public int TodoId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool UseTristate { get; set; }
        public string TaskOrder
        {
            get => Enum.GetName(typeof(TaskOrderPattern), TaskOrderEnum);
            set => TaskOrderEnum = (TaskOrderPattern)Enum.Parse(typeof(TaskOrderPattern), value);
        }
        public int IconPatternId { get; set; } //key IconPatternMaster
        public int ColorPatternId { get; set; } //key ColorPatternMaster

        [Ignored]
        internal IconSetting IconPattern { get; set; }
        [Ignored]
        internal ColorSetting ColorPattern { get; set; }
        [Ignored]
        [IgnoreMap]
        public TaskOrderPattern TaskOrderEnum { get; set; }
    }

    public class Task : RealmObject
    {
        [Indexed]
        public int TodoId { get; set; }
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Status
        {
            get => Enum.GetName(typeof(TaskStatus), StatusEnum);
            set => StatusEnum = (TaskStatus)Enum.Parse(typeof(TaskStatus), value);
        }
        public int DisplayOrder { get; set; }

        [Ignored]
        [IgnoreMap]
        public TaskStatus StatusEnum { get; set; }
    }

    #region enum
    public enum MenuBarPosition
    {
        Left,
        Right,
    }

    public enum TabPosition
    {
        Top,
        Left,
        Right,
        Bottom,
    }

    public enum TaskOrderPattern
    {
        Registered,
        Name,
    }

    public enum TaskStatus
    {
        Unchecked,
        Checked,
        Canceled,
    }

    public enum ViewingPage
    {
        Todo,
        TabList,
    }

    public enum MenuBarIcon
    {
        TabList,
        NewTask,
        Up,
        Down,
        TabSetting,
        NewTab,
    }
    #endregion

    #endregion

    #region Target Object
    public class TodoTask
    {
        [IgnoreMap]
        public ReactiveProperty<int> TaskId { get; private set; }
        [IgnoreMap]
        public ReactiveProperty<string> Name { get; private set; }
        [IgnoreMap]
        public ReactiveProperty<TaskStatus> Status { get; private set; }
        [IgnoreMap]
        public ReactiveProperty<int> DisplayOrder { get; private set; }

        internal int inner_TaskId { get => TaskId.Value; set => TaskId.Value = value; }
        internal string inner_Name { get => Name.Value; set => Name.Value = value; }
        internal TaskStatus inner_Status { get => Status.Value; set => Status.Value = value; }
        internal int inner_DisplayOrder { get => DisplayOrder.Value; set => DisplayOrder.Value = value; }

        internal TodoTask() : this(-1, string.Empty, TaskStatus.Unchecked, -1)
        {
        }

        public TodoTask(int taskId, string task, TaskStatus status, int order)
        {
            TaskId = new ReactiveProperty<int>(taskId);
            Name = new ReactiveProperty<string>(task);
            Status = new ReactiveProperty<TaskStatus>(status);
            DisplayOrder = new ReactiveProperty<int>(order);
        }
    }

    public class TodoItem
    {
        [IgnoreMap]
        public ReactiveProperty<int> TodoId { get; private set; }
        [IgnoreMap]
        public ReactiveProperty<string> Name { get; private set; }
        [IgnoreMap]
        public ReactiveProperty<int> DisplayOrder { get; private set; }
        [IgnoreMap]
        public ReactiveProperty<bool> IsActive { get; private set; }
        [IgnoreMap]
        public ReactiveProperty<bool> UseTristate { get; private set; }
        [IgnoreMap]
        public ReactiveProperty<TaskOrderPattern> TaskOrder { get; private set; }
        public int IconPatternId { get; set; } //key IconPatternMaster
        public int ColorPatternId { get; set; } //key ColorPatternMaster
        public IconSetting IconPattern { get; set; }
        public ColorSetting ColorPattern { get; set; }

        internal int inner_TodoId { get => TodoId.Value; set => TodoId.Value = value; }
        internal string inner_Name { get => Name.Value; set => Name.Value = value; }
        internal int inner_DisplayOrder { get => DisplayOrder.Value; set => DisplayOrder.Value = value; }
        internal bool inner_IsActive { get => IsActive.Value; set => IsActive.Value = value; }
        internal bool inner_UseTristate { get => UseTristate.Value; set => UseTristate.Value = value; }
        internal TaskOrderPattern inner_TaskOrder { get => TaskOrder.Value; set => TaskOrder.Value = value; }

        internal TodoItem() : this(-1, string.Empty, -1, false)
        {
        }

        public TodoItem(int todoId, string todo) : this(todoId, todo, -1, true)
        {
        }

        public TodoItem(int todoId, string todo, int order) : this(todoId, todo, order, true)
        {
        }

        public TodoItem(int todoId, string todo, int order, bool active)
        {
            TodoId = new ReactiveProperty<int>(todoId);
            Name = new ReactiveProperty<string>(todo);
            DisplayOrder = new ReactiveProperty<int>(order);
            IsActive = new ReactiveProperty<bool>(active);
            UseTristate = new ReactiveProperty<bool>(false);
            TaskOrder = new ReactiveProperty<TaskOrderPattern>(TaskOrderPattern.Registered);
        }
    }

    public class IconSetting
    {
        public string CheckedIcon { get; set; }
        public string CanceledIcon { get; set; }
    }

    public class ColorSetting
    {
        public Color PageBasicBackgroundColor { get; set; }   //ContentPage,ContentView
        public Color ViewBasicTextColor { get; set; }         //Label,Button,Entry,etc...
        public Color NavigationBarBackgroundColor { get; set; }
        public Color NavigationBarTextColor { get; set; }
        public Color MenuBarBackgroundColor { get; set; }
        public Color TabListViewBackgroundColor { get; set; }
        public Color TabListViewSeparatorColor { get; set; }
        public Color TabListViewCellColor { get; set; }
        public Color TabListViewTextColor { get; set; }
        public Color TabListViewCellSelectedColor { get; set; }
        public Color TabListViewTextSelectedColor { get; set; }
        public Color TabBarBackgroundColor { get; set; }
        public Color TabBarTextColor { get; set; }
        public Color TodoViewBackgroundColor { get; set; }
        public Color TodoViewSeparatorColor { get; set; }
        public Color TodoViewCellColor { get; set; }
        public Color TodoViewTextColor { get; set; }
        public Color TodoViewCheckAreaBackgroundColor { get; set; }
        public Color TodoViewCellSelectedColor { get; set; }
        public Color TodoViewTextSelectedColor { get; set; }
        public Color SlideMenuBackgroundColor { get; set; }
        public Color SlideMenuCellBackgroundColor { get; set; }
        public Color SlideMenuCellTextColor { get; set; }
        public Color SlideMenuPickerBackgroundColor { get; set; }
        public Color SlideMenuPickerTextColor { get; set; }
        public Color SwitchOnColor { get; set; }             //Switch,SwitchCell
    }
    #endregion

    #region Helper Class
    public class UpdateMapper<TObj>
    {
        public List<Action<TObj>> MapList { get; } = new List<Action<TObj>>();
    }
    #endregion
}
