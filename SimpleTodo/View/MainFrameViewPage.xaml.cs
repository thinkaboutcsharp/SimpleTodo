using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Anywhere;

namespace SimpleTodo
{
    public partial class MainFrameViewPage : MasterDetailPage
    {
        private IReactiveSource<object> slideMenuInitializeSource;

        public MainFrameViewPage()
        {
            InitializeComponent();

            IsPresentedChanged += (_s, _e) => { if (!IsPresented) slideMenuInitializeSource.Send(null); };

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget(RxSourceEnum.DirectTabSettingMenu, (int _) => IsPresented = true);
            slideMenuInitializeSource = router.AddReactiveSource<object>(RxSourceEnum.SlideMenuInitialize);
        }
    }
}
