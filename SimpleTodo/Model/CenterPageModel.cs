using System;
using Reactive.Bindings;

namespace SimpleTodo
{
	public class CenterPageModel : ModelBase
    {
		public ReactiveProperty<ColorSetting> ColorPattern = new ReactiveProperty<ColorSetting>();

		public CenterPageModel(RealmAccess realm) : base(realm)
        {
        }
    }
}
