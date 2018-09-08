using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Reactive.Bindings;
using stt = System.Threading.Tasks;

namespace SimpleTodo
{
    public class BaseSlideMenuPageModel : ModelBase
    {
        #region Properties to bind
        public ReactiveProperty<SlideMenuMode> MenuMode { get; } = new ReactiveProperty<SlideMenuMode>(SlideMenuMode.Main);
        public ReactiveProperty<string> TabSettingTitle { get; } = new ReactiveProperty<string>(string.Empty);

        public ReactiveCommand<SettingTab> TabSettingTransitCommand { get; } = new ReactiveCommand<SettingTab>();
        public SettingTab CurrentTabParameter { get; } = SettingTab.Current;
        public SettingTab AllTabParameter { get; } = SettingTab.All;

        public ReactiveCommand TabSettingReturnCommand { get; } = new ReactiveCommand();

        public ReactiveCommand TabInsertPositionChangeCommand { get; } = new ReactiveCommand();

        public ReactiveProperty<bool> StartAtTabList { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> UseBigIcon { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> RightMenuBarInLandscape { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<bool> UseTristate { get; }
        public ReactiveProperty<TaskOrderList> OrderPattrn { get; }
        public ReactiveProperty<IconSetting> IconPattern { get; }
        public ReactiveProperty<ColorSetting> ColorPattern { get; }
        public ReactiveProperty<bool> SuitAll { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<IReadOnlyList<TaskOrderList>> OrderPatternCandidates { get; } = new ReactiveProperty<IReadOnlyList<TaskOrderList>>();
        public ReactiveProperty<IReadOnlyList<IconSetting>> IconPatternCandidates { get; } = new ReactiveProperty<IReadOnlyList<IconSetting>>();
        public ReactiveProperty<IReadOnlyList<ColorSetting>> ColorPatternCandidates { get; } = new ReactiveProperty<IReadOnlyList<ColorSetting>>();

        public ReactiveProperty<SettingTab> CurrentTabSetting { get; } = new ReactiveProperty<SettingTab>(SettingTab.Current);
        #endregion

        private MenuBarIconSizeChangedObservable menuBarIconSizeChangedSource = new MenuBarIconSizeChangedObservable();
        public IObservable<bool> MenuBarIconSizeChangedSource { get => menuBarIconSizeChangedSource; }

        public BaseSlideMenuPageModel(RealmAccess realm) : base(realm)
        {
            TabSettingTransitCommand.Subscribe(s => OnTabSettingTransit(s));
            TabSettingReturnCommand.Subscribe(() => OnTabSettingReturn());

            StartAtTabList.Value = realm.IsBeginFromTabList();
            UseBigIcon.Value = realm.IsBigIcon();
            RightMenuBarInLandscape.Value = realm.GetMenuBarPosition() == MenuBarPosition.Right ? true : false;

            OrderPatternCandidates.Value = realm.GetTaskOrderList();

            var defaultOrder = OrderPatternCandidates.Value.Where(p => p.TaskOrder == realm.GetDefaultTaskOrder()).Select(p => p).FirstOrDefault();
            UseTristate = new ReactiveProperty<bool>(realm.GetDefaultUseTristate());
            OrderPattrn = new ReactiveProperty<TaskOrderList>(defaultOrder);
            ColorPattern = new ReactiveProperty<ColorSetting>(realm.GetDefaultColorPatternAsync().Result);
            IconPattern = new ReactiveProperty<IconSetting>(realm.GetDefaultIconPatternAsync().Result);

            SuitAll.Value = false;

            IconPatternCandidates.Value = realm.GetIconPatternAllAsync().Result;
            ColorPatternCandidates.Value = realm.GetColorPatternAllAsync().Result;

            UseBigIcon.Subscribe(async s => await ToggleUseBigIcon(s));
        }

        private void OnTabSettingTransit(SettingTab setting)
        {
            switch (setting)
            {
                case SettingTab.Current:
                    TabSettingTitle.Value = "このタブ";
                    break;
                case SettingTab.All:
                    TabSettingTitle.Value = "全部のタブ";
                    break;
            }
            CurrentTabSetting.Value = setting;
            MenuMode.Value = SlideMenuMode.TabSetting;
        }

        public async stt.Task ToggleUseBigIcon(bool useBigSize)
        {
            menuBarIconSizeChangedSource.Send(useBigSize);
            await realm.UseBigIconAsync(useBigSize);
        }

        public void TransitCurrentTabSetting(int todoId)
        {
            //タブ一覧から直で開いて表示→終わったら裏の現在のタブに戻す
        }

        public void TransitAllTabSetting()
        {

        }

        private void OnTabSettingReturn()
        {
            MenuMode.Value = SlideMenuMode.Main;
            SuitAll.Value = false;
        }

        public stt.Task OnCentralViewChange(TodoItem currentTodo)
        {
            //タブを切り替えた時に裏で走る
            return stt.Task.Run(() =>
            {

            });
        }
    }

    public enum SlideMenuMode
    {
        Main,
        TabSetting,
    }
}
