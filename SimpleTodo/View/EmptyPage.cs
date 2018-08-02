using System;

using Xamarin.Forms;

namespace SimpleTodo
{
    public class EmptyPage : ContentPage, ITabPage
    {
        private Tab setting;
        public Tab Tab { get => setting; set { setting = value; Title = value.Name; } }
    }
}

