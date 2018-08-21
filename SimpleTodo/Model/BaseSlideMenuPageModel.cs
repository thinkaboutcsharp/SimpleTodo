using System;
using System.Collections.Generic;
using System.Windows.Input;
using Reactive.Bindings;

namespace SimpleTodo
{
    public class BaseSlideMenuPageModel : ModelBase
    {
        #region Properties to bind
        public ReactiveProperty<SlideMenuMode> MenuMode { get; } = new ReactiveProperty<SlideMenuMode>(SlideMenuMode.Main);
        public ReactiveProperty<string> TabSettingTitle { get; } = new ReactiveProperty<string>(string.Empty);

        public ReactiveCommand<TabSetting> TabSettingTransitCommand { get; } = new ReactiveCommand<TabSetting>();
        public TabSetting CurrentTabParameter { get; } = TabSetting.Current;
        public TabSetting AllTabParameter { get; } = TabSetting.All;

        public ReactiveCommand TabSettingReturnCommand { get; } = new ReactiveCommand();

        public ReactiveProperty<bool> StartAtTabList { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> UseBigIcon { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> RightMenuBarInLandscape { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<bool> UseTristate { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> SuitAll { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<IReadOnlyList<TaskOrderList>> OrderPattern { get; } = new ReactiveProperty<IReadOnlyList<TaskOrderList>>();
        public ReactiveProperty<IReadOnlyList<ColorSetting>> ColorPattern { get; } = new ReactiveProperty<IReadOnlyList<ColorSetting>>();
        public ReactiveProperty<IReadOnlyList<IconSetting>> IconPattern { get; } = new ReactiveProperty<IReadOnlyList<IconSetting>>();
        public ReactiveProperty<TabSetting> CurrentTabSetting { get; } = new ReactiveProperty<TabSetting>(TabSetting.Current);
        #endregion

        public BaseSlideMenuPageModel(RealmAccess realm) : base(realm)
        {
            TabSettingTransitCommand.Subscribe(s => OnTabSettingTransit(s));
            TabSettingReturnCommand.Subscribe(() => OnTabSettingReturn());

            StartAtTabList.Value = realm.IsBeginFromTabList();
            UseBigIcon.Value = realm.IsBigIcon();
            RightMenuBarInLandscape.Value = realm.GetMenuBarPosition() == MenuBarPosition.Right ? true : false;

            UseTristate.Value = realm.GetDefaultUseTristate();
            SuitAll.Value = false;

            OrderPattern.Value = realm.GetTaskOrderList();
            ColorPattern.Value = realm.GetColorPatternAllAsync().Result;
            IconPattern.Value = realm.GetIconPatternAllAsync().Result;
        }

        private void OnTabSettingTransit(TabSetting setting)
        {
            switch (setting)
            {
                case TabSetting.Current:
                    TabSettingTitle.Value = "このタブ";
                    break;
                case TabSetting.All:
                    TabSettingTitle.Value = "全部のタブ";
                    break;
            }
            CurrentTabSetting.Value = setting;
            MenuMode.Value = SlideMenuMode.TabSetting;
        }

        private void OnTabSettingReturn()
        {
            MenuMode.Value = SlideMenuMode.Main;
        }

        public void OnCenterViewChanged(ColorSetting colorSetting)
        {
            this.ColorSetting.Value = colorSetting;
        }
    }

    public enum TabSetting
    {
        All,
        Current
    }

    public enum SlideMenuMode
    {
        Main,
        TabSetting,
    }
}
