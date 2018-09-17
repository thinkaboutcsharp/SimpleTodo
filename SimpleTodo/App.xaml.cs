using System;
using System.Reflection;
using System.Linq;
using stt = System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using EventRouting;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SimpleTodo
{
    public partial class App : Application
    {
        private CentralViewChangeObserver centralViewChangeTarget;

        public App()
        {
            Resources = new ResourceDictionary();

            var reaction = new ReactionRouter();
            Application.Current.Properties.Add(nameof(ReactionRouter), reaction);

            var request = new RequestRouter();
            Application.Current.Properties.Add(nameof(RequestRouter), request);

            var commonSettings = new CommonSettings();
            Application.Current.Properties.Add(nameof(CommonSettings), commonSettings);

            //SimpleTodo.SQLite.SQLiteAccess.PrepareMapping();
            //var dataAccess = new SimpleTodo.SQLite.SQLiteAccess();
            //Application.Current.Properties.Add(AppExtention.DatabaseOptionKey, AppExtention.DatabaseOption.SQLite);
            SimpleTodo.Realm.RealmAccess.PrepareMapping();
            var dataAccess = new SimpleTodo.Realm.RealmAccess();
            Application.Current.Properties.Add(AppExtention.DatabaseOptionKey, AppExtention.DatabaseOption.Realm);
            Application.Current.Properties.Add(nameof(IDataAccess), dataAccess);

            var initialColor = dataAccess.GetDefaultColorPatternAsync().Result;
            InitColorResource(initialColor).Wait();

            centralViewChangeTarget = new CentralViewChangeObserver(async (todo) => await SetColorResourceAsync(todo.ColorPattern));
            reaction.AddReactiveTarget(RxSourceEnum.CentralViewChange, centralViewChangeTarget);

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
            Application.Current.DataAccess().CloseConnection();
        }

        protected override async void OnResume()
        {
            await Application.Current.DataAccess().OpenConnectionAsync();
        }
    }
}
