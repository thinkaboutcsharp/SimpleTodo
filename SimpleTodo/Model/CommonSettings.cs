using System;
namespace SimpleTodo
{
    public class CommonSettings
    {
        public const int UndefinedId = -1;

        public MenuBarPosition HorizontalMenuBarPosition { get; set; } = MenuBarPosition.Left;
        public TabPosition NewTabPosition { get; set; } = TabPosition.Top;
        public bool BeginFromTabList { get; set; } = false;
    }
}
