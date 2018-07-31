using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class TemplatePage : ContentPage
    {
        public TemplatePage(string title)
        {
            InitializeComponent();

            Title = title;

            var items = new List<TodoItem>
            {
                new TodoItem("abc----------------", false),
                new TodoItem("def------------", false),
                new TodoItem("ghi------", false),
                new TodoItem(string.Empty, false)
            };
            lvw_TodoList.ItemsSource = items;

            lvw_TodoList.ItemTapped += (sender, e) =>
            {
                var item = (TodoItem)lvw_TodoList.SelectedItem;
                DisplayAlert("item", item.Text, "OK");
                item.Done = !item.Done;
            };
        }

        public void OnDelete(object sender, EventArgs args)
        {
            var item = (TodoItem)((MenuItem)sender).CommandParameter;
            DisplayAlert("delete", item.Text, "OK");
        }
    }
}
