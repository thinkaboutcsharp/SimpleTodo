using System;
using Xamarin.Forms;
using EventRouting;

namespace SimpleTodo
{
    public static class AppExtention
    {
        public static ReactionRouter ReactionRouter(this Application application)
            => (ReactionRouter)application.Properties[nameof(ReactionRouter)];

        public static RequestRouter RequestRouter(this Application application)
            => (RequestRouter)application.Properties[nameof(RequestRouter)];

        public static MenuBarView MenuBarView(this Application application)
            => (MenuBarView)application.Properties[nameof(MenuBarView)];

        public static CommonSettings CommonSettings(this Application application)
            => (CommonSettings)application.Properties[nameof(CommonSettings)];

        public static RealmAccess RealmAccess(this Application application)
            => (RealmAccess)application.Properties[nameof(RealmAccess)];
    }
}
