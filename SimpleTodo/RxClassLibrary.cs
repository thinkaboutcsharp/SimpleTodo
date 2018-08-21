using System;
using System.Drawing;

namespace SimpleTodo
{
    #region TodoTabNew
    class TodoTabNewObservable : ObservableBase<string>
    {
    }

    class TodoTabNewObserver : ObserverBase<string>
    {
        public TodoTabNewObserver(Action<string> action) : base(action) { }
    }
    #endregion

    #region TabJumping
    class TabJumpingObservable : ObservableBase<int>
    {
    }

    class TabJumpingObserver : ObserverBase<int>
    {
        public TabJumpingObserver(Action<int> action) : base(action) { }
    }
    #endregion

    #region DirectTabSetting
    class DirectTabSettingObservable : ObservableBase<object>
    {
    }

    class DirectTabSettingObserver : ObserverBase<object>
    {
        public DirectTabSettingObserver(Action<object> action) : base(action) { }
    }
    #endregion

    #region TabListTransit
    class TabListTransitObservable : ObservableBase<object>
    {
    }

    class TabListTransitObserver : ObserverBase<object>
    {
        public TabListTransitObserver(Action<object> action) : base(action) { }
    }
    #endregion

    #region VisibleSwitchOnOff
    class VisibleSwitchOnOffObservable : ObservableBase<bool>
    {
    }

    class VisibleSwitchOnOffObserver : ObserverBase<bool>
    {
        public VisibleSwitchOnOffObserver(Action<bool> action) : base(action) { }
    }
    #endregion

    #region ClearSelection
    class ClearSelectionObservable : ObservableBase<Color>
    {
    }

    class ClearSelectionOvserver : ObserverBase<Color>
    {
        public ClearSelectionOvserver(Action<Color> action) : base(action) { }
    }
    #endregion

    #region SlideMenuInitialize
    class SlideMenuInitializeObservable : ObservableBase<object>
    {
    }

    class SlideMenuInitializeObserver : ObserverBase<object>
    {
        public SlideMenuInitializeObserver(Action<object> action) : base(action) { }
    }
    #endregion

    #region PageRotation
    class PageRotetionObservable : ObservableBase<PageDirectionEnum>
    {
    }

    class PageRotationOvserver : ObserverBase<PageDirectionEnum>
    {
        public PageRotationOvserver(Action<PageDirectionEnum> action) : base(action) { }
    }
    #endregion

    #region TabViewAppearing
    class TabMaintenanceDisppearingObservable : ObservableBase<object>
    {
    }

    class TabViewAppearingObserver : ObserverBase<object>
    {
        public TabViewAppearingObserver(Action<object> action) : base(action) { }
    }
    #endregion

    #region TodoTabVisibleChange
    class ChangeVisibilityObservable : ObservableBase<(int, bool)>
    {
    }
    #endregion

    #region TabUpDown
    class TabUpDownObservable : ObservableBase<(TabUpDown, int)>
    {
    }
    #endregion

    #region MenuBarIconSizeChange
    class MenuBarIconSizeChangedOvserver : ObserverBase<bool>
    {
        public MenuBarIconSizeChangedOvserver(Action<bool> action) : base(action) { }
    }
    #endregion

    #region CentralViewChange
    class CentralViewChangeObserver : ObserverBase<ColorSetting>
    {
        public CentralViewChangeObserver(Action<ColorSetting> action) : base(action) { }
    }
    #endregion

}
