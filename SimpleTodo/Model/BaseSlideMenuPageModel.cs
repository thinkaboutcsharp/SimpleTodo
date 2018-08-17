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
        public ReactiveProperty<IReadOnlyList<OrderItem>> OrderPattern { get; } = new ReactiveProperty<IReadOnlyList<OrderItem>>();
        public ReactiveProperty<IReadOnlyList<ListItem>> ColorPattern { get; } = new ReactiveProperty<IReadOnlyList<ListItem>>();
        public ReactiveProperty<IReadOnlyList<ListItem>> IconPattern { get; } = new ReactiveProperty<IReadOnlyList<ListItem>>();
        public ReactiveProperty<TabSetting> CurrentTabSetting { get; } = new ReactiveProperty<TabSetting>(TabSetting.Current);
        #endregion

        public BaseSlideMenuPageModel(RealmAccess realm) : base(realm)
        {
            TabSettingTransitCommand.Subscribe(s => OnTabSettingTransit(s));
            TabSettingReturnCommand.Subscribe(() => OnTabSettingReturn());

            //DBから
            StartAtTabList.Value = false;
            UseBigIcon.Value = false;
            RightMenuBarInLandscape.Value = true;

            UseTristate.Value = true;
            SuitAll.Value = false;

            OrderPattern.Value = new List<OrderItem>
            {
                new OrderItem { Text = "登録順" },
                new OrderItem { Text = "名前順"}
            };
            ColorPattern.Value = new List<ListItem>
            {
                new ListItem { Text = "赤" },
                new ListItem { Text = "青" },
                new ListItem { Text = "黄" }
            };
            IconPattern.Value = new List<ListItem>
            {
                new ListItem { Text = "✓" },
                new ListItem { Text = "猫" }
            };
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

        public class ListItem
        {
            public string Text { get; set; }
        }

        public class OrderItem
        {
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
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
