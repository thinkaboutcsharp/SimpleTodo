using System;
using Reactive.Bindings;

namespace SimpleTodo
{
    public abstract class ModelBase
    {
        protected IDataAccess realm;

        public ReactiveProperty<ColorSetting> ColorSetting = new ReactiveProperty<ColorSetting>();

        public ModelBase(IDataAccess realm)
        {
            this.realm = realm;
        }
    }
}
