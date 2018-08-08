using System;

using Xamarin.Forms;

namespace SimpleTodo.Droid
{
    public class FitMasterDetailPage : ContentPage
    {
        public FitMasterDetailPage()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

