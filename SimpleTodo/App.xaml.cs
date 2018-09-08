using System;
using System.Reflection;
using System.Linq;
using stt = System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using RxRouting;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SimpleTodo
{
    public partial class App : Application
    {
        private CentralViewChangeObserver centralViewChangeTarget;

        public App()
        {
            Resources = new ResourceDictionary();

            var router = new ReactionRouter();
            Application.Current.Properties.Add(nameof(ReactionRouter), router);

            var commonSettings = new CommonSettings();
            Application.Current.Properties.Add(nameof(CommonSettings), commonSettings);

            RealmAccess.PrepareMapping();
            var realmAccess = new RealmAccess();
            Application.Current.Properties.Add(nameof(RealmAccess), realmAccess);

            var initialColor = realmAccess.GetDefaultColorPatternAsync().Result;
            InitColorResource(initialColor).Wait();

            centralViewChangeTarget = new CentralViewChangeObserver(async (todo) => await SetColorResourceAsync(todo.ColorPattern));
            router.AddReactiveTarget(RxSourceEnum.CentralViewChange, centralViewChangeTarget);

            InitializeComponent();

            var menuBar = new MenuBarView();
            Application.Current.Properties.Add(nameof(MenuBarView), menuBar);

            MainPage = new MainFrameViewPage();
        }

        protected override void OnStart()
        {
            //起動画面とかいる？
        }

        private async stt.Task InitColorResource(ColorSetting setting)
        {
            await SetColorResource((r, n, c) => r.Add(n, c), setting);
        }

        private async stt.Task SetColorResourceAsync(ColorSetting setting)
        {
            await SetColorResource((r, n, c) => r[n] = c, setting);
        }

        private stt.Task SetColorResource(Action<ResourceDictionary, string, Xamarin.Forms.Color> action, ColorSetting setting)
        {
            return stt.Task.Run(() =>
            {
                var properties = typeof(ColorSetting).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    var color = (Xamarin.Forms.Color)property.GetValue(setting);
                    action(Resources, property.Name, color);
                }
            });
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
