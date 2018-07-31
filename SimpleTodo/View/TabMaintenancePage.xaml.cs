using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class TabMaintenancePage : global::Xamarin.Forms.ContentPage
    {
        public TabMaintenancePage()
        {
            InitializeComponent();

            var tabList = new List<TabItem>
            {
                new TabItem { Text = "ようこそ"},
                new TabItem {Text = "こんにちは"}
            };

            lvw_TabMaintenance.ItemsSource = tabList;
        }

        void OnTapped(object sender, TappedEventArgs args)
        {
            DisplayAlert("tapped", args.ToString(), "OK");            
        }

        void OnVisibleChanged(object sender, EventArgs args)
        {

        }

        void OnDelete(object sender, EventArgs args)
        {

        }
    }

    class TabItem
    {
        public string Text { get; set; }
    }
}
