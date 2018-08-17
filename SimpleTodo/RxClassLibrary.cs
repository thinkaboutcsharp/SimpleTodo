using System;
using System.Drawing;

namespace SimpleTodo
{
    class TodoTabNewObserver : ObserverBase<string>
    {
        public TodoTabNewObserver(Action<string> action) : base(action) { }
    }

    class TabJumpingObserver : ObserverBase<int>
    {
        public TabJumpingObserver(Action<int> action) : base(action) { }
    }

    class PageRotationOvserver : ObserverBase<PageDirectionEnum>
    {
        public PageRotationOvserver(Action<PageDirectionEnum> action) : base(action) { }
    }

    class TabListTransitObservable : ObservableBase<object>
    {
    }

    class TabViewAppearingObserver : ObserverBase<object>
    {
        public TabViewAppearingObserver(Action<object> action) : base(action) { }
    }

    class DirectTabSettingObservable : ObservableBase<object>
    {
    }

    class TabMaintenanceDisppearingObservable : ObservableBase<object>
    {
    }

    class TabJumpingObservable : ObservableBase<int>
    {
    }

    class VisibleSwitchOnOffObservable : ObservableBase<bool>
    {
    }

    class ClearSelectionObservable : ObservableBase<Color>
    {
    }

    class ChangeVisibilityObservable : ObservableBase<(int, bool)>
    {
    }

    class TabUpDownObservable : ObservableBase<(TabUpDown, int)>
    {
    }

    class MenuBarIconSizeChangedOvserver : ObserverBase<bool>
    {
        public MenuBarIconSizeChangedOvserver(Action<bool> action) : base(action) { }
    }

    class VisibleSwitchOnOffObserver : ObserverBase<bool>
    {
        public VisibleSwitchOnOffObserver(Action<bool> action) : base(action) { }
    }

    class ClearSelectionOvserver : ObserverBase<Color>
    {
        public ClearSelectionOvserver(Action<Color> action) : base(action) { }
    }

        class DirectTabSettingObserver : ObserverBase<object>
    {
        public DirectTabSettingObserver(Action<object> action) : base(action) { }
    }

    class SlideMenuInitializeObserver : ObserverBase<object>
    {
        public SlideMenuInitializeObserver(Action<object> action) : base(action) { }
    }

}
