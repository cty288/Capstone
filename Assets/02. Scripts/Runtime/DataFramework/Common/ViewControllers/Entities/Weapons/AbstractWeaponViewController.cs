using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities.Weapons;
using Runtime.DataFramework.Entities;

namespace Runtime.DataFramework.ViewControllers.Entities.Weapons
{
    public abstract class AbstractWeaponViewController<T> : AbstractBasicEntityViewController<T, IWeaponEntityModel>, IWeaponViewController
                where T : class, IWeaponEntity, new()
    {
        public IWeaponEntity WeaponEntity => BindedEntity;

        [field: SerializeField]
        public int BaseDamage { get; }

        protected IWeaponEntityModel weaponEntityModel;

        protected override void Awake()
        {
            base.Awake();

            weaponEntityModel = this.GetModel<IWeaponEntityModel>();
        }

        protected override void OnBindEntityProperty()
        {
            Bind("BaseDamage", BindedEntity.GetBaseDamage());
        }

        protected override IEntity OnInitEntity()
        {
            WeaponBuilder<T> builder = weaponEntityModel.GetWeaponBuilder<T>(1);
            return OnInitEnemyEntity(builder);
        }

        protected abstract IWeaponEntity OnInitEnemyEntity(WeaponBuilder<T> builder);


        protected dynamic GetBaseDamage(dynamic info)
        {
            return info.BaseDamage;
        }
    }

}
