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

            MainPage = new MainFrameViewPage();
        }

        protected override void OnStart()
        {
            var router = new ReactionRouter();
            Application.Current.Properties.Add(nameof(ReactionRouter), router);

            var menuBar = new MenuBarView();
            Application.Current.Properties.Add(nameof(MenuBarView), menuBar);

            var commonSettings = new CommonSettings();
            Application.Current.Properties.Add(nameof(CommonSettings), commonSettings);

            RealmAccess.PrepareMapping();
            var realmAccess = new RealmAccess();
            Application.Current.Properties.Add(nameof(RealmAccess), realmAccess);
        }

        protected override void OnSleep()
        {
            Application.Current.RealmAccess().CloseConnection();
        }

        protected override async void OnResume()
        {
            await Application.Current.RealmAccess().OpenConnectionAsync();
        }
    }
}
