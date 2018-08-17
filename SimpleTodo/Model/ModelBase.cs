using System;
namespace SimpleTodo
{
    public abstract class ModelBase
    {
		protected RealmAccess realm;

        public ModelBase(RealmAccess realm)
        {
			this.realm = realm;
        }
    }
}
