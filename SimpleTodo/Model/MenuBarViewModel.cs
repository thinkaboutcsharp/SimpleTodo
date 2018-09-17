using System;
using EventRouting;

namespace SimpleTodo
{
	public class MenuBarViewModel : ModelBase
    {
        public bool NeedBigIcon { get; set; }

		public MenuBarViewModel(IDataAccess realm) : base(realm)
        {
            NeedBigIcon = realm.IsBigIcon();
        }
    }
}
