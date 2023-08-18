using MikroFramework.BindableProperty;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.Utilities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.Enemies;
using Runtime.DataFramework.Entities;
using System.Collections.Generic;
using Runtime.DataFramework.Properties.CustomProperties;
using MikroFramework.Event;
using System;

namespace Runtime.DataFramework.Entities.Weapons
{
    public interface IWeaponEntity : IHaveCustomProperties, IHaveTags
    {
        public BindableProperty<int> GetBaseDamage();
        // public BindableProperty<int> GetAttackSpeed();
        // public BindableProperty<float> GetRange();
        // public BindableProperty<int> GetMagazineSize();
        // public BindableProperty<float> GetReloadTime();
        // public BindableProperty<int> GetBulletPerShot();
        // public BindableProperty<int> GetSpread();
        // public BindableProperty<float> GetRecoil();
        // public BindableProperty<float> GetChargeTime();
        // public BindableProperty<float> GetBulletSpeed();
    }

    public abstract class WeaponEntity<T> : AbstractBasicEntity, IWeaponEntity where T : WeaponEntity<T>, new()
    {
        public BindableProperty<int> GetBaseDamage()
        {
            throw new NotImplementedException();
        }
    }
}