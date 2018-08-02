using System;
using System.Collections.Generic;
using Reactive.Bindings;

using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class TemplateView : ContentView, ITabPage
    {
        private TemplateViewModel model = new TemplateViewModel();

        public Tab Tab { get; set; }

        public void SetCurrentTodo(Tab setting)
        {
            Tab = setting;
            BindingContext = setting;

            var todoList = model.LoadTodo(setting.TodoId);
            lvw_TodoList.ItemsSource = todoList;
        }

        public TemplateView()
        {
            InitializeComponent();

            lvw_TodoList.ItemTapped += (sender, e) =>
            {
                var item = (TemplateViewModel.TodoTask)lvw_TodoList.SelectedItem;
                item.Checked.Value = !item.Checked.Value;
            };

            //上からリクエストがあればリストを取得するため、最初は何もしない
        }

        private void OnDelete(object sender, EventArgs args)
        {
            var item = (TemplateViewModel.TodoTask)((MenuItem)sender).CommandParameter;
        }
    }
}
