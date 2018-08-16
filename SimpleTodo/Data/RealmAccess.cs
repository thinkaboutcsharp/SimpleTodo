using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using stt = System.Threading.Tasks;

using PCLStorage;
using SQLite;
using Reactive.Bindings;

namespace SimpleTodo
{
    public class SqliteAccess
    {
        private SQLiteAsyncConnection sqlite;
        private string dbFile;

        #region Master Table
        private SystemSettings systemSettings;
        private MenuIconMaster menuIconMaster;
        private List<IconPatternMaster> iconPatternMaster;
        private List<ColorPatternMaster> colorPatternMaster;
        #endregion

        #region Connect
        public SqliteAccess()
        {
            var folder = FileSystem.Current.LocalStorage;
            dbFile = Path.Combine(folder.Path, "item.db3");
            OpenConnection().Wait();
        }

        public void CloseConnection()
        {
            sqlite.GetConnection().Close();
            sqlite = null;
        }

        public async stt.Task OpenConnection()
        {
            if (!File.Exists(dbFile))
            {
                await CopyEmbeddedDb(dbFile);
            }
            sqlite = new SQLiteAsyncConnection(dbFile);
        }
        #endregion

        #region Initialize
        public bool IsSameSystemVersion(string latestVersion)
        {
            //ズレたままは進めないので同期

            var version = sqlite.Table<SystemVersion>().FirstOrDefaultAsync().Result;
            if (version.Version == latestVersion) return true;
            else return false;
        }

        public void UpgradeMaster()
        {
            //終わるまで進めないので同期

            //コピー用の一時ファイル作成
            var localMasterFile = Path.Combine(FileSystem.Current.LocalStorage.Path, "master.db3");
            CopyEmbeddedDb(localMasterFile).Wait();
            var masterDb = new SQLiteConnection(localMasterFile);

            //既存レコード削除(スキーマも変わっている可能性あり)
            sqlite.DropTableAsync<IconPatternMaster>().Wait();
            sqlite.DropTableAsync<ColorPatternMaster>().Wait();
            sqlite.CreateTablesAsync<IconPatternMaster, ColorPatternMaster>().Wait();

            //新レコードコピー
            sqlite.InsertAllAsync(masterDb.Table<IconPatternMaster>().AsEnumerable()).Wait();
            sqlite.InsertAllAsync(masterDb.Table<ColorPatternMaster>().AsEnumerable()).Wait();

            //一時ファイル削除
            masterDb.Close();
            File.Delete(localMasterFile);
        }

        public async stt.Task UpdateSystemVersion(string newVersion)
        {
            await sqlite.UpdateAsync(new SystemVersion { Version = newVersion });
        }
        #endregion

        #region DML
        public async stt.Task Insert(object record) => await sqlite.InsertAsync(record);
        public async stt.Task Insert<T>(IEnumerable<T> records) => await sqlite.InsertAllAsync(records);
        public async stt.Task Update(object record) => await sqlite.UpdateAsync(record);
        public async stt.Task Update<T>(IEnumerable<T> records) => await sqlite.UpdateAllAsync(records);
        public async stt.Task Delete(object record) => await sqlite.DeleteAsync(record);
        public async stt.Task Truncate<T>() => await sqlite.ExecuteAsync(string.Format("Delete From {0}", typeof(T).Name));
        #endregion

        #region Query
        public async stt.Task<IconPatternMaster> GetDefaultIconPattern()
        {
            if (iconPatternMaster == null)
            {
                iconPatternMaster = await sqlite.Table<IconPatternMaster>().ToListAsync();
            }
            if (systemSettings == null)
            {
                systemSettings = await sqlite.Table<SystemSettings>().FirstAsync();
            }

            var defaultPattern = iconPatternMaster.Where(p => p.IconId == systemSettings.DefaultIconPattern).Select(p => p).First();
            return defaultPattern;
        }

        public async stt.Task<IEnumerable<TodoItem>> SelectTodoAll()
        {
            var todoAll = new List<TodoItem>();
            var todoTable = await sqlite.Table<Todo>().OrderBy(todo => todo.DisplayOrder).ToListAsync();
            foreach (var todo in todoTable)
            {

            }
            return todoAll;
        }

        public async stt.Task<IEnumerable<Task>> SelectTaskAll(int todoId)
        {
            var taskAll = await sqlite.Table<Task>().Where(task => task.TodoId == todoId).OrderBy(task => task.DisplayOrder).ToListAsync();
            return taskAll;
        }
        #endregion

        #region Private
        private async stt.Task CopyEmbeddedDb(string copyPath)
        {
            var assembly = this.GetType().Assembly;

            using (var db = File.Create(dbFile))
            using (var master = assembly.GetManifestResourceStream("SimpleTodo.Data.master.db3"))
            {
                await master.CopyToAsync(db);
                await db.FlushAsync();
            }
        }
        #endregion
    }

    #region Schema Object
    public class SystemVersion
    {
        public string Version { get; set; }
    }

    public class LastPage
    {
        public string LastViewing { get; set; } //enum ViewingPage
        public int LastFocus { get; set; }
        public int New { get; set; }
    }

    public class TodoIdMaster
    {
        public int LastTodoId { get; set; }
    }

    public class TaskIdMaster
    {
        [PrimaryKey]
        public int TodoId { get; set; }
        public int LastTaskId { get; set; }
    }

    public class SystemSettings
    {
        public bool UseBigIcon { get; set; }
        public bool BeginFromTabList { get; set; }
        public string HorizontalMenuBarPosition { get; set; } //enum MenuBarPosition
        public string NewTabPosition { get; set; } //enum TabPosition
        public bool DefaultUseTristate { get; set; }
        public string DefaultTaskOrder { get; set; } //enum TaskOrderPattern
        public int DefaultIconPattern { get; set; } //IconPatternMaster
        public int DefaultColorPattern { get; set; } //ColorPatternMaster
    }

    public class MenuIconMaster
    {
        public string TabListIcon { get; set; }
        public string NewTaskIcon { get; set; }
        public string Up { get; set; }
        public string Down { get; set; }
        public string TabSetting { get; set; }
        public string NewTabIcon { get; set; }
    }

    public class IconPatternMaster
    {
        [PrimaryKey]
        public int IconId { get; set; }
        public string CheckedIcon { get; set; }
        public string CanceledIcon { get; set; }
    }

    public class ColorPatternMaster
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

    public class Todo
    {
        [PrimaryKey]
        public int TodoId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool UseTristate { get; set; }
        public string TaskOrder { get; set; } //enum TaskOrderPattern
        public int IconPatternId { get; set; } //key IconPatternMaster
        public int ColorPatternId { get; set; } //key ColorPatternMaster
    }

    public class Task
    {
        [PrimaryKey]
        public string TaskKey
        {
            //複合キーは使えない
            get => string.Format("{0:00000}+{1:000000}", TodoId, TaskId);
            set { string[] ids = value.Split('+'); TodoId = int.Parse(ids[0]); TaskId = int.Parse(ids[1]); }
        }
        [Ignore]
        public int TodoId { get; set; }
        [Ignore]
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } //enum TaskStatus
        public int DisplayOrder { get; set; }
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
    #endregion

    #endregion

    #region Target Object
    public class TodoTask
    {
        public ReactiveProperty<int> TaskId { get; private set; }
        public ReactiveProperty<string> Task { get; private set; }
        public ReactiveProperty<TaskStatus> Status { get; private set; }

        public TodoTask(int taskId, string task, TaskStatus status)
        {
            TaskId = new ReactiveProperty<int>(taskId);
            Task = new ReactiveProperty<string>(task);
            Status = new ReactiveProperty<TaskStatus>(status);
        }
    }

    public class TodoItem
    {
        public ReactiveProperty<int> TodoId { get; private set; }
        public ReactiveProperty<string> Todo { get; private set; }
        public ReactiveProperty<int> Order { get; private set; }
        public ReactiveProperty<bool> IsActive { get; private set; }

        public TodoItem(int todoId, string todo) : this(todoId, todo, -1, true)
        {
        }

        public TodoItem(int todoId, string todo, int order) : this(todoId, todo, order, true)
        {
        }

        public TodoItem(int todoId, string todo, int order, bool active)
        {
            TodoId = new ReactiveProperty<int>(todoId);
            Todo = new ReactiveProperty<string>(todo);
            Order = new ReactiveProperty<int>(order);
            IsActive = new ReactiveProperty<bool>(active);
        }
    }

    public class TabSettings
    {
        public int TodoId { get; set; }
        public string Name { get; set; }
        public IconSetting IconPattern { get; set; }
        public ColorSetting ColorPattern { get; set; }

        public int DislayOrder { get; set; }
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
}
