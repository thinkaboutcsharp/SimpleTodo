using System;
using Reactive.Bindings;

namespace SimpleTodo
{
    public abstract class ModelBase
    {
        protected IDataAccess dataAccess;

        public ReactiveProperty<ColorSetting> ColorSetting = new ReactiveProperty<ColorSetting>();

        public ModelBase(IDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public virtual void InitModel() {}
    }
}
