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

        public CenterPage(Page childPage) : base(childPage)
        {
            InitializeComponent();

            BindingContext = model;

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget(RxSourceEnum.TabListTransit, async (object _) => await PushAsync(new TabMaintenancePage()));
            router.AddReactiveTarget(RxSourceEnum.TabJumping, async (int _) => await PopAsync());
        }
    }
}
