using System;
using Xamarin.Forms;
using Anywhere;

namespace SimpleTodo
{
    public static class AppExtention
    {
        internal const string DatabaseOptionKey = "DatabaseOption";
        internal enum DatabaseOption
        {
            Realm,
            SQLite,
        }

        public static ReactionRouter ReactionRouter(this Application application)
            => (ReactionRouter)application.Properties[nameof(ReactionRouter)];

        public static RequestRouter RequestRouter(this Application application)
            => (RequestRouter)application.Properties[nameof(RequestRouter)];

        public static MenuBarView MenuBarView(this Application application)
            => (MenuBarView)application.Properties[nameof(MenuBarView)];

        public static CommonSettings CommonSettings(this Application application)
            => (CommonSettings)application.Properties[nameof(CommonSettings)];

        public static IDataAccess DataAccess(this Application application)
            => (IDataAccess)application.Properties[nameof(IDataAccess)];

        public static Color ColorSetting(this Application application, string colorName)
            => (Color)application.Resources[colorName];
    }
}
