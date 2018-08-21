using System;
using Reactive.Bindings;

namespace SimpleTodo
{
    public class OriginTabPageModel : ModelBase
    {
        public OriginTabPageModel(RealmAccess realm) : base(realm)
        {
            ColorSetting.Value = realm.GetDefaultColorPatternAsync().Result;
        }
    }
}
