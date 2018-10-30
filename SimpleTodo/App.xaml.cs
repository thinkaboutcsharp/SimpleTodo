using System;
using System.Reflection;
using System.Linq;
using stt = System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Anywhere;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SimpleTodo
{
    public partial class App : Application
    {
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

            var initialColor = dataAccess.GetDefaultColorPattern();
            InitColorResource(initialColor);

            reaction.AddReactiveTarget(RxSourceEnum.CentralViewChange, (TodoItem todo) => SetColorResource(todo.ColorPattern));

            InitializeComponent();

            var menuBar = new MenuBarView();
            Application.Current.Properties.Add(nameof(MenuBarView), menuBar);

            MainPage = new MainFrameViewPage();
        }

        protected override void OnStart()
        {
            //起動画面とかいる？
        }

        private void InitColorResource(ColorSetting setting)
        {
            SetColorResource((r, n, c) => r.Add(n, c), setting);
        }

        private void SetColorResource(ColorSetting setting)
        {
            SetColorResource((r, n, c) => r[n] = c, setting);
        }

        private void SetColorResource(Action<ResourceDictionary, string, Xamarin.Forms.Color> action, ColorSetting setting)
        {
				var properties = typeof(ColorSetting).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(System.Drawing.Color));
                foreach (var property in properties)
                {
                    Xamarin.Forms.Color color = (System.Drawing.Color)property.GetValue(setting);
                    action(Resources, property.Name, color);
                }
        }

        protected override void OnSleep()
        {
            Application.Current.DataAccess().CloseConnection();
        }

        protected override void OnResume()
        {
            Application.Current.DataAccess().OpenConnection();
        }
    }
}
