using System;
using RxRouting;

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
