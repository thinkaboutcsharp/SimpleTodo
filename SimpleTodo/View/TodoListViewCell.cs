using System;

using Xamarin.Forms;

namespace SimpleTodo
{
    public class TodoListViewCell : ContentView
    {
        public TodoListViewCell()
        {
            Content = new Label { Text = "Hello ContentView" };
        }
    }
}

