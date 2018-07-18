using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class MainFrameView : MasterDetailPage
    {
        public MainFrameView()
        {
            InitializeComponent();

            Master = new BaseSlideMenuPage();

            var tabView = new TabViewPage();
            tabView.Children.Insert(0, new TemplatePage("さいしょ"));
            tabView.CurrentPage = tabView.Children[0];
            Detail = new NavigationPage(tabView);
        }
    }
}
