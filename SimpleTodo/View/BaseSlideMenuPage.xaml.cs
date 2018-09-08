using System;
using System.Collections.Generic;
using System.Windows.Input;
using Reactive.Bindings;
using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class BaseSlideMenuPage : ContentPage
    {
        private BaseSlideMenuPageModel model = new BaseSlideMenuPageModel(Application.Current.RealmAccess());

        private DirectTabSettingObserver directTabSettingTarget;
        private SlideMenuInitializeObserver slideMenuInitializeTarget;
        private CentralViewChangeObserver centralViewChangeTarget;

        public BaseSlideMenuPage()
        {
            InitializeComponent();

            BindingContext = model;

            model.ColorPatternCandidates.Subscribe(cp =>
            {
                lay_ColorPattern.Children.Clear();
                foreach (var pattern in cp)
                {
                    lay_ColorPattern.Children.Add(new Label { Text = pattern.PageBasicBackgroundColor.Name });
                }
            });
            model.IconPatternCandidates.Subscribe(ip =>
            {
                lay_IconPattern.Children.Clear();
                foreach (var pattern in ip)
                {
                    lay_IconPattern.Children.Add(new Label { Text = pattern.CheckedIcon });
                }
            });

            directTabSettingTarget = new DirectTabSettingObserver(target =>
            {
                if (target == CommonSettings.UndefinedId) model.TransitAllTabSetting();
                else model.TransitCurrentTabSetting(target);
            });
            slideMenuInitializeTarget = new SlideMenuInitializeObserver(_ =>
            {
                model.TabSettingReturnCommand.Execute();
            });
            centralViewChangeTarget = new CentralViewChangeObserver(async todo => await model.OnCentralViewChange(todo));

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget(RxSourceEnum.DirectTabSettingMenu, directTabSettingTarget);
            router.AddReactiveTarget(RxSourceEnum.SlideMenuInitialize, slideMenuInitializeTarget);
            router.AddReactiveTarget(RxSourceEnum.CentralViewChange, centralViewChangeTarget);
            router.AddReactiveSource(RxSourceEnum.MenuBarIconSizeChange, model.MenuBarIconSizeChangedSource);
        }
    }
}
