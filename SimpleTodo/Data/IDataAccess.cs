using System;
using System.Drawing;
using System.Collections.Generic;
using stt = System.Threading.Tasks;

using Reactive.Bindings;
using AutoMapper;

namespace SimpleTodo
{
    public interface IDataAccess
    {
        void OpenConnection();
        void CloseConnection();

        string GetSystemVersion();
        string GetMenuBarIconFile(MenuBarIcon menu);
        bool GetDefaultUseTristate();
        void SetDefaultUseTristateAsync(bool value);
        TaskOrderPattern GetDefaultTaskOrder();
        void SetDefaultTaskOrderAsync(TaskOrderPattern order);
        bool IsBigIcon();
        void UseBigIconAsync(bool usage);
        bool IsBeginFromTabList();
        void BeginFromTabListAsync(bool begin);

        MenuBarPosition GetMenuBarPosition();
        void SetMenuBarPositionAsync(MenuBarPosition position);
        TabPosition GetNewTabPosition();
        void SetNewTabPositionAsync(TabPosition position);
        ViewingPage GetLastPage();
        void SetLastPageAsync(ViewingPage viewing);
        int GetLastTabIndex();
        void SetLastTabIndexAsync(int focus);
        int GetOriginTabIndex();
        void SetOriginTabIndexAsync(int origin);

        IReadOnlyList<TaskOrderList> GetTaskOrderList();
        int GetNewTodoId();
        int GetNewTaskId(int todoId);
        stt.Task<IEnumerable<TodoItem>> SelectTodoAllAsync();
        stt.Task<IEnumerable<TodoTask>> SelectTaskAllAsync(int todoId);

        TodoItem AddTodoAsync(TodoItem todoItem);
        TodoTask AddTaskAsync(int todoId, TodoTask todoTask);
        void ChangeVisibilityAsync(int todoId, bool visibile);
        void RenameTodoAsync(int todoId, string newName);
        void RenameTaskAsync(int todoId, int taskId, string newName);
        void ReorderTodoAsync(IEnumerable<TodoItem> todoItems);
        void ReorderTaskAsync(int todoId, IEnumerable<TodoTask> todoTasks);
        void ToggleTaskStatusAsync(int todoId, int taskId, TaskStatus status);
        void UpdateTodoAsync(TodoItem todoItem);
        void UpdateTaskAsync(int todoId, TodoTask todoTask);
        void DeleteTodoAsync(int todoId);
        void DeleteTaskAsync(int todoId, int taskId);

        IReadOnlyList<IconSetting> GetIconPatternAll();
        IReadOnlyList<ColorSetting> GetColorPatternAll();
        IReadOnlyList<TaskOrderList> GetTaskOrderDisplayName();
        IconSetting GetDefaultIconPattern();
        ColorSetting GetDefaultColorPattern();
        TodoItem GetDefaultTabSetting(int newTodoId, string newName);
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
        SwitchOnOff,
    }

    public static class EnumExtension
    {
        public static string EnumName<TEnum>(this TEnum value) => Enum.GetName(typeof(TEnum), value);
        public static TEnum EnumValue<TEnum>(this string name) => (TEnum)Enum.Parse(typeof(TEnum), name);
    }
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
        public bool IndependentSetting { get; set; }

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
            IndependentSetting = false;
        }
    }

    public class IconSetting
    {
        public int IconId { get; set; }
        public string CheckedIcon { get; set; }
        public string CanceledIcon { get; set; }
    }

    public class ColorSetting
    {
        public int ColorId { get; set; }
        public Color PageBasicBackgroundColor { get; set; }   //ContentPage,ContentView
        public Color ViewBasicTextColor { get; set; }         //Label,Entry,etc...
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
        public Color SlideMenuCellDescriptionColor { get; set; }
        public Color SlideMenuPickerBackgroundColor { get; set; }
        public Color SlideMenuPickerTextColor { get; set; }
        public Color SwitchOnColor { get; set; }             //Switch,SwitchCell
        public Color SwitchTintColor { get; set; }
        public Color SwitchThumbColor { get; set; }
        public Color PickerTintColor { get; set; }
        public Color PickerBackgroundColor { get; set; }
        public Color EditBoxBackgroundColor { get; set; }  //Entry,Editor
        public Color ButtonTextColor { get; set; }
        public Color ButtonBackgroundColor { get; set; }
        /* 将来機能 */
        public Color PickerPageTitleColor { get; set; }
        public Color PickerPageHeaderBackgroundColor { get; set; }
        public Color PickerPageBackgroundColor { get; set; }
        public Color PickerPageTextColor { get; set; }
    }

    public class TaskOrderList
    {
        public TaskOrderPattern TaskOrder { get; set; }
        public string DisplayName { get; set; }
    }
    #endregion

    #region Helper Class
    public class UpdateMapper<TObj>
    {
        public List<Action<TObj>> MapList { get; } = new List<Action<TObj>>();
    }
    #endregion
}
