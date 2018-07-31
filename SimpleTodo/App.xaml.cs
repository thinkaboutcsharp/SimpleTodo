using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using RxRouting;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SimpleTodo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var router = new ReactionRouter();
            Application.Current.Properties.Add(nameof(ReactionRouter), router);

            MainPage = new MainFrameViewPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
