using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Builders
{
    public class WeaponBuilder<T> : EntityBuilder<WeaponBuilder<T>, T> where T : class, IEntity, new()
    {

        public WeaponBuilder()
        {
            CheckEntity();
        }

        public WeaponBuilder<T> SetBaseDamage(int baseDamage)
        {
            SetProperty<int>(new PropertyNameInfo(PropertyName.base_damage), baseDamage);
            return this;
        }
        
        public WeaponBuilder<T> SetAllBasics(int baseDamage)
        {
            SetBaseDamage(baseDamage);
            return this;
        }
        
        public override void RecycleToCache()
        {
            SafeObjectPool<WeaponBuilder<T>>.Singleton.Recycle(this);
        }

         public static WeaponBuilder<T> Allocate(int rarity)
         {
             WeaponBuilder<T> target = SafeObjectPool<WeaponBuilder<T>>.Singleton.Allocate();
             target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
             return target;
         }
    }
}