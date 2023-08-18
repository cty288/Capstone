using System.Collections.Generic;
using System.Linq;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;

namespace Runtime.DataFramework.Entities.Weapons
{
    public class WeaponBuilder<T> : EntityBuilder<WeaponBuilder<T>, T> where T : class, IEntity, new()
    {

        public WeaponBuilder()
        {
            CheckEntity();
        }

        public WeaponBuilder<T> SetBaseDamage(int baseDamage, IPropertyDependencyModifier<int> modifier = null)
        {
            SetProperty<int>(new PropertyNameInfo(PropertyName.base_damage), baseDamage, modifier);
            return this;
        }

        public WeaponBuilder<T> SetBaseDamageModifier(IPropertyDependencyModifier<int> modifier = null)
        {
            SetModifier(new PropertyNameInfo(PropertyName.danger), modifier);
            return this;
        }

        public WeaponBuilder<T> SetAllBasics(int baseDamage)
        {
            SetBaseDamage(baseDamage);
            return this;
        }

        public WeaponBuilder<T> SetAllBasicsModifiers(IPropertyDependencyModifier<int> baseDamage)
        {
            SetBaseDamageModifier(baseDamage);
            return this;
        }

        public override void RecycleToCache()
        {
            SafeObjectPool<WeaponBuilder<T>>.Singleton.Recycle(this);
        }

        // public static EnemyBuilder<T> Allocate(int rarity)
        // {
        //     EnemyBuilder<T> target = SafeObjectPool<EnemyBuilder<T>>.Singleton.Allocate();
        //     target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
        //     return target;
        // }
    }
}