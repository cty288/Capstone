using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Weapons.Model.Base;

namespace Runtime.Weapons.ViewControllers.Base
{
    public abstract class AbstractWeaponViewController<T> : AbstractBasicEntityViewController<T, IWeaponModel>
        where T : class, IWeaponEntity, new()
    {
        protected override void OnEntityStart() {
            OnStart();
        }

        protected abstract void OnStart();
    }

}
