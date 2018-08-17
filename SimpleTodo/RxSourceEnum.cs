using System;
namespace SimpleTodo
{
    public enum RxSourceEnum : int
    {
        TodoTabNew,
        TodoTabVisibleChange,

        PageRotation,

        TabListTransit,
        TabListClose,
        TabJumping,
        TabUpDown,

        DirectTabSettingMenu,
        SlideMenuInitialize,

        MenuBarIconSizeChange,

        ClearListViewSelection,
        VisibleSwitchOnOff,
    }

    public enum TabUpDown
    {
        Up,
        Down
    }
}
