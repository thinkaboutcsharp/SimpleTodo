using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class MainFrameViewPage : MasterDetailPage
    {
        private DirectTabSettingObserver directTabSettingTarget;
        private SlideMenuInitializeObservable slideMenuInitializeSource;

        public MainFrameViewPage()
        {
            InitializeComponent();

            IsPresentedChanged += (_s, _e) => { if (!IsPresented) slideMenuInitializeSource.Send(null); };

            directTabSettingTarget = new DirectTabSettingObserver(_ => IsPresented = true);
            slideMenuInitializeSource = new SlideMenuInitializeObservable();

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget((int)RxSourceEnum.DirectTabSettingMenu, directTabSettingTarget);
            router.AddReactiveSource((int)RxSourceEnum.SlideMenuInitialize, slideMenuInitializeSource);
        }
    }
}
