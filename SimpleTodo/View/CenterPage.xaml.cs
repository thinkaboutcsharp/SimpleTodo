using System;
using System.Collections.Generic;
using stt = System.Threading.Tasks;

using Anywhere;
using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class CenterPage : NavigationPage
    {
        private CenterPageModel model = new CenterPageModel(Application.Current.DataAccess());

        private TabListTransitObserver tabListTransitTarget;
        private TabJumpingObserver tabJumpingTarget;

        public CenterPage(Page childPage) : base(childPage)
        {
            InitializeComponent();

            BindingContext = model;

            tabListTransitTarget = new TabListTransitObserver(async _ => await PushAsync(new TabMaintenancePage()));
            tabJumpingTarget = new TabJumpingObserver(async _ => await PopAsync());

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget(RxSourceEnum.TabListTransit, tabListTransitTarget);
            router.AddReactiveTarget(RxSourceEnum.TabJumping, tabJumpingTarget);
        }
    }
}
