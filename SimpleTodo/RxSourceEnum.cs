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

        MenuBarIconSizeChange,
    }
}
