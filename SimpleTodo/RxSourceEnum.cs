using System;
namespace SimpleTodo
{
    public enum RxSourceEnum : int
    {
        TodoTabNew,
        TodoTabVisibleChange,
        TodoTabNewOnList,

        PageRotation,

        TodoTaskNew,
        TodoTaskUpDown,

        TabListTransit,
        TabListClose,
        TabJumping,
        TabUpDown,
        TabRemove,
        TabTitleChange,

        CentralViewChange,

        DirectTabSettingMenu,
        SlideMenuInitialize,

        MenuBarIconSizeChange,
        TabSettingChange,
        MenuBarPositionChange,

        ClearListViewSelection,
        VisibleSwitchOnOff,

        SuspendApp,
    }

    public enum UpDown
    {
        Up,
        Down
    }

    public enum SettingTab
    {
        All,
        Current,
    }

    public enum ListType
    {
        Task,
        Todo,
    }

    public enum Suspend
    {
        Sleep,
        Resume,
    }
}
