using System;
using System.Drawing;
using Anywhere;

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
    class DirectTabSettingObservable : ObservableBase<int>
    {
    }

    class DirectTabSettingObserver : ObserverBase<int>
    {
        public DirectTabSettingObserver(Action<int> action) : base(action) { }
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

    class ChangeVisibilityObserver : ObserverBase<(int, bool)>
    {
        public ChangeVisibilityObserver(Action<(int, bool)> action) : base(action) { }
    }
    #endregion

    #region TabUpDown
    class TabUpDownObservable : ObservableBase<(UpDown, int)>
    {
    }
    class TabUpDownObserver : ObserverBase<(UpDown, int)>
    {
        public TabUpDownObserver(Action<(UpDown, int)> action) : base(action) { }
    }    #endregion

    #region MenuBarIconSizeChange
    class MenuBarIconSizeChangedObservable : ObservableBase<bool>
    {
    }
    class MenuBarIconSizeChangedOvserver : ObserverBase<bool>
    {
        public MenuBarIconSizeChangedOvserver(Action<bool> action) : base(action) { }
    }
    #endregion

    #region CentralViewChange
    class CentralViewChangeObservable : ObservableBase<TodoItem>
    {
    }
    class CentralViewChangeObserver : ObserverBase<TodoItem>
    {
        public CentralViewChangeObserver(Action<TodoItem> action) : base(action) { }
    }
    #endregion

    #region TabNewOnList
    class TabNewOnListObservable : ObservableBase<string>
    {
    }
    class TabNewOnListObserver : ObserverBase<string>
    {
        public TabNewOnListObserver(Action<string> action) : base(action) { }
    }
    #endregion

    #region TabTitleChange
    class TabTitleChangeObservable : ObservableBase<(int, string)>
    {
    }
    class TabTitleChangeObserver : ObserverBase<(int, string)>
    {
        public TabTitleChangeObserver(Action<(int, string)> action) : base(action) { }
    }
    #endregion

    #region TabRemove
    class TabRemoveObservable : ObservableBase<int>
    {
    }
    class TabRemoveObserver : ObserverBase<int>
    {
        public TabRemoveObserver(Action<int> action) : base(action) { }
    }
    #endregion
}
