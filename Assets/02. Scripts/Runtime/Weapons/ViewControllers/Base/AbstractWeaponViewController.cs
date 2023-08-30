using System;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.ViewControllers;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.Model.Properties;

namespace Runtime.Weapons.ViewControllers.Base
{
    public abstract class AbstractWeaponViewController<T> : AbstractResourceViewController<T>
        where T : class, IWeaponEntity, new() {

        private IWeaponModel weaponModel;
        private void Awake() {
            weaponModel = this.GetModel<IWeaponModel>();
        }

        protected override IEntity OnBuildNewEntity()
        {
            WeaponBuilder<T> builder = weaponModel.GetWeaponBuilder<T>();
            return OnInitWeaponEntity(builder);
        }
        
        protected abstract IEntity OnInitWeaponEntity(WeaponBuilder<T> builder);
    }
}
