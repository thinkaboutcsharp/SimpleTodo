using System;
using System.Collections.Generic;
using System.Windows.Input;
using Reactive.Bindings;
using Xamarin.Forms;

using Anywhere;

namespace SimpleTodo
{
    public partial class BaseSlideMenuPage : ContentPage
    {
        private BaseSlideMenuPageModel model = new BaseSlideMenuPageModel(Application.Current.DataAccess());

        private IRequester<TodoItem> requester;

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

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget(
                RxSourceEnum.DirectTabSettingMenu,
                (int target) =>
                {
                    if (target == CommonSettings.UndefinedId)
                    {
                        var setting = requester.RequestSingle(CommonSettings.UndefinedId);
                        model.TransitAllTabSetting(setting);
                    }
                    else
                    {
                        var setting = requester.RequestSingle(target);
                        model.TransitCurrentTabSetting(setting);
                    }
                }
            );
            router.AddReactiveTarget(RxSourceEnum.SlideMenuInitialize, (object _) => model.TabSettingReturnCommand.Execute());
            router.AddReactiveTarget(RxSourceEnum.CentralViewChange, async (TodoItem todo) => await model.OnCentralViewChange(todo));
            model.MenuBarIconSizeChangedSource = router.AddReactiveSource<bool>(RxSourceEnum.MenuBarIconSizeChange);

            var request = Application.Current.RequestRouter();
            requester = request.CreateRequester<TodoItem>(RqSourceEnum.TabSetting);

            model.InitModel();
        }
    }
}
