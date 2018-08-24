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
    }

    public enum UpDown
    {
        Up,
        Down
    }

    public enum DirectTabSettingTarget
    {
        All,
        Current,
    }

    public static class RxSourceEx
    {
        public static int Value(this RxSourceEnum field) => (int)field;
    }
}
