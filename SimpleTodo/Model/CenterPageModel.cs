using System;
using Reactive.Bindings;

namespace SimpleTodo
{
    public class CenterPageModel : ModelBase
    {
        public CenterPageModel(RealmAccess realm) : base(realm)
        {
        }

        public void OnCentralViewChanged(ColorSetting colorSetting)
        {
            this.ColorSetting.Value = colorSetting;
        }
    }
}
