using System;
using Reactive.Bindings;

namespace SimpleTodo
{
    public abstract class ModelBase
    {
        protected RealmAccess realm;

        public ReactiveProperty<ColorSetting> ColorSetting = new ReactiveProperty<ColorSetting>();

        public ModelBase(RealmAccess realm)
        {
            this.realm = realm;
        }
    }
}
