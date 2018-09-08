using System;
using EventRouting;

namespace SimpleTodo
{
	public class MenuBarViewModel : ModelBase
    {
        public bool NeedBigIcon { get; set; }

		public MenuBarViewModel(RealmAccess realm) : base(realm)
        {
            NeedBigIcon = realm.IsBigIcon();
        }
    }
}
