using System;
using Reactive.Bindings;

namespace SimpleTodo
{
    public class OriginTabPageModel : ModelBase
    {
        public OriginTabPageModel(IDataAccess realm) : base(realm)
        {
            ColorSetting.Value = realm.GetDefaultColorPatternAsync().Result;
        }
    }
}
