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

            model.ColorPattern.Subscribe(cp =>
            {
                lay_ColorPattern.Children.Clear();
                foreach (var pattern in cp)
                {
                    lay_ColorPattern.Children.Add(new Label { Text = pattern.PageBasicBackgroundColor.Name });
                }
            });
            model.IconPattern.Subscribe(ip =>
            {
                lay_IconPattern.Children.Clear();
                foreach (var pattern in ip)
                {
                    lay_IconPattern.Children.Add(new Label { Text = pattern.CheckedIcon });
                }
            });

            directTabSettingTarget = new DirectTabSettingObserver(_ =>
            {
                model.TabSettingTransitCommand.Execute(TabSetting.Current);
            });
            slideMenuInitializeTarget = new SlideMenuInitializeObserver(_ =>
            {
                model.TabSettingReturnCommand.Execute();
            });
            centralViewChangeTarget = new CentralViewChangeObserver(c => model.OnCenterViewChanged(c));

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget((int)RxSourceEnum.DirectTabSettingMenu, directTabSettingTarget);
            router.AddReactiveTarget((int)RxSourceEnum.SlideMenuInitialize, slideMenuInitializeTarget);
            router.AddReactiveTarget((int)RxSourceEnum.CentralViewChange, centralViewChangeTarget);
        }
    }
}
