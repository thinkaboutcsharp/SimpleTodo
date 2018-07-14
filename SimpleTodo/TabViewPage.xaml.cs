using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class TabViewPage : TabbedPage
    {
        public TabViewPage()
        {
            InitializeComponent();
        }

        void OnTabChanged(object sender, EventArgs args)
        {
            Title = CurrentPage.Title;
        }
    }
}
