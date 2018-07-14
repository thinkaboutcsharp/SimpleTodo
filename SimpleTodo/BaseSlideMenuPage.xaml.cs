using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class BaseSlideMenuPage : global::Xamarin.Forms.ContentPage
    {
        public BaseSlideMenuPage()
        {
            InitializeComponent();

            var items = new List<SlideItem>()
            {
                new SlideItem { Id = Menu.TabMaintenance, Text = "タブ一覧" },
                new SlideItem { Id = Menu.TabSettings, Text = "タブ設定" },
                new SlideItem { Id = Menu.CommonSettings, Text = "共通設定" },
                new SlideItem { Id = Menu.PrintPdf, Text = "PDF" },
                new SlideItem { Id = Menu.AboutApp, Text = "このアプリについて" },
            };
            lvw_BaseSlideMenu.ItemsSource = items;
        }

        void OnMenuTapped(object sender, TappedEventArgs args)
        {
            var item = (SlideItem)lvw_BaseSlideMenu.SelectedItem;

            switch (item.Id)
            {
                case Menu.TabMaintenance:
                    break;
                case Menu.TabSettings:
                    break;
                case Menu.CommonSettings:
                    break;
                case Menu.PrintPdf:
                    break;
                case Menu.AboutApp:
                    break;
            }

            lvw_BaseSlideMenu.SelectedItem = null;

            var parent = (MasterDetailPage)this.Parent;
            parent.IsPresented = false;
        }
    }

    enum Menu
    {
        TabMaintenance,
        TabSettings,
        CommonSettings,
        PrintPdf,
        AboutApp
    }

    class SlideItem
    {
        public Menu Id { get; set; }
        public string Text { get; set; }
    }
}
