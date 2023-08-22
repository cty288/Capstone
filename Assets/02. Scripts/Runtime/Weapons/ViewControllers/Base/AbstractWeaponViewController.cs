using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.Model.Properties;

namespace Runtime.Weapons.ViewControllers.Base
{
    public abstract class AbstractWeaponViewController<T> : AbstractBasicEntityViewController<T, IWeaponModel>
        // , IWeaponViewController
        where T : class, IWeaponEntity, new()
    {
        protected override IEntity OnInitEntity()
        {
            WeaponBuilder<T> builder = entityModel.GetWeaponBuilder<T>();
            return OnInitWeaponEntity(builder);
        }
        
        protected abstract IEntity OnInitWeaponEntity(WeaponBuilder<T> builder);
    }
}
