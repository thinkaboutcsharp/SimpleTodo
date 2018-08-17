using System;
using System.Collections.Generic;

using RxRouting;
using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class CenterPage : NavigationPage
    {
        private TabListTransitObserver tabListTransitTarget;
        private TabJumpingOvserver tabJumpingTarget;

        public CenterPage(Page childPage) : base(childPage)
        {
            InitializeComponent();

            tabListTransitTarget = new TabListTransitObserver(async _ => await PushAsync(new TabMaintenancePage()));
            tabJumpingTarget = new TabJumpingOvserver(async _ => await PopAsync());

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget<object>((int)RxSourceEnum.TabListTransit, tabListTransitTarget);
            router.AddReactiveTarget((int)RxSourceEnum.TabJumping, tabJumpingTarget);
        }

        class TabListTransitObserver : ObserverBase<object>
        {
            public TabListTransitObserver(Action<object> action) : base(action) { }
        }

        class TabJumpingOvserver : ObserverBase<int>
        {
            public TabJumpingOvserver(Action<int> action) : base(action) { }
        }
    }
}
