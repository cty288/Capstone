﻿using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Properties;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Weapons.Model.Base
{
    public struct OnWeaponRecoilEvent
    {
        public Vector3 recoilVector;
        public float snappiness;
        public float returnSpeed;
    }
    
    public interface IWeaponEntity : IResourceEntity, IHaveCustomProperties, IHaveTags {
        public IBaseDamage GetBaseDamage();
        public IAttackSpeed GetAttackSpeed();
        public IRange GetRange();
        public IAmmoSize GetAmmoSize();
        public IReloadSpeed GetReloadSpeed();
        public IBulletsPerShot GetBulletsPerShot();
        public ISpread GetSpread();
        public IRecoil GetRecoil();
        public IScopeRecoil GetScopeRecoil();
        public IBulletSpeed GetBulletSpeed();
        public IChargeSpeed GetChargeSpeed();
        
        public BindableProperty<int> CurrentAmmo { get; set; }
        // public GunRecoil GunRecoilScript { get; set; }
        
        public void Reload();

        public void OnRecoil(bool isScopedIn);
    }
    
    public abstract class WeaponEntity<T> :  ResourceEntity<T>, IWeaponEntity  where T : WeaponEntity<T>, new() {
        private IBaseDamage baseDamageProperty;
        private IAttackSpeed attackSpeedProperty;
        private IRange rangeProperty;
        private IAmmoSize ammoSizeProperty;
        private IReloadSpeed reloadSpeedProperty;
        private IBulletsPerShot bulletsPerShotProperty;
        private ISpread spreadProperty;
        private IRecoil recoilProperty;
        private IScopeRecoil scopeRecoilProperty;
        private IBulletSpeed bulletSpeedProperty;
        private IChargeSpeed chargeSpeedProperty;
        [field: ES3Serializable]
        public BindableProperty<int> CurrentAmmo { get; set; } = new BindableProperty<int>(0);

        
        public abstract int Width { get; }

        protected override ConfigTable GetConfigTable() {
            
            return ConfigDatas.Singleton.WeaponEntityConfigTable;
        }


        public override ResourceCategory GetResourceCategory() {
            return ResourceCategory.Weapon;
        }

        protected override void OnEntityStart(bool isLoadedFromSave) {
            base.OnEntityStart(isLoadedFromSave);
            baseDamageProperty = GetProperty<IBaseDamage>();
            attackSpeedProperty = GetProperty<IAttackSpeed>();
            rangeProperty = GetProperty<IRange>();
            ammoSizeProperty = GetProperty<IAmmoSize>();
            reloadSpeedProperty = GetProperty<IReloadSpeed>();
            bulletsPerShotProperty = GetProperty<IBulletsPerShot>();
            spreadProperty = GetProperty<ISpread>();
            recoilProperty = GetProperty<IRecoil>();
            scopeRecoilProperty = GetProperty<IScopeRecoil>();
            bulletSpeedProperty = GetProperty<IBulletSpeed>();
            chargeSpeedProperty = GetProperty<IChargeSpeed>();
            
            if (!isLoadedFromSave) { //otherwise it is managed by es3
                CurrentAmmo.Value = ammoSizeProperty.RealValue.Value;
            }
           
        }
        
        public override void OnDoRecycle() {
            SafeObjectPool<T>.Singleton.Recycle(this as T);
        }

        public override void OnRecycle() {
            CurrentAmmo.UnRegisterAll();
        }

        protected override void OnEntityRegisterAdditionalProperties() {
            base.OnEntityRegisterAdditionalProperties();
            RegisterInitialProperty<IBaseDamage>(new BaseDamage());
            RegisterInitialProperty<IAttackSpeed>(new AttackSpeed());
            RegisterInitialProperty<IRange>(new Range());
            RegisterInitialProperty<IAmmoSize>(new AmmoSize());
            RegisterInitialProperty<IReloadSpeed>(new ReloadSpeed());
            RegisterInitialProperty<IBulletsPerShot>(new BulletsPerShot());
            RegisterInitialProperty<ISpread>(new Spread());
            RegisterInitialProperty<IRecoil>(new Recoil());
            RegisterInitialProperty<IScopeRecoil>(new ScopeRecoil());
            RegisterInitialProperty<IBulletSpeed>(new BulletSpeed());
            RegisterInitialProperty<IChargeSpeed>(new ChargeSpeed());
        }
        
        public IBaseDamage GetBaseDamage()
        {
            return baseDamageProperty;
        }
        
        public IAttackSpeed GetAttackSpeed()
        {
            return attackSpeedProperty;
        }
        
        public IRange GetRange()
        {
            return rangeProperty;
        }
        
        public IAmmoSize GetAmmoSize()
        {
            return ammoSizeProperty;
        }
        
        public IReloadSpeed GetReloadSpeed()
        {
            return reloadSpeedProperty;
        }
        
        public IBulletsPerShot GetBulletsPerShot()
        {
            return bulletsPerShotProperty;
        }
        
        public ISpread GetSpread()
        {
            return spreadProperty;
        }
        
        public IRecoil GetRecoil()
        {
            return recoilProperty;
        }

        public IScopeRecoil GetScopeRecoil()
        {
            return scopeRecoilProperty;
        }

        public IBulletSpeed GetBulletSpeed()
        {
            return bulletSpeedProperty;
        }
        
        public IChargeSpeed GetChargeSpeed()
        {
            return chargeSpeedProperty;
        }



        public void Reload() {
            CurrentAmmo.Value = ammoSizeProperty.RealValue.Value;
        }

        public void OnRecoil(bool isScopedIn)
        {
            if (isScopedIn)
            {
                this.SendEvent<OnWeaponRecoilEvent>(new OnWeaponRecoilEvent()
                {
                    recoilVector = scopeRecoilProperty.GetRecoilVector(),
                    snappiness = scopeRecoilProperty.GetSnappiness(),
                    returnSpeed = scopeRecoilProperty.GetReturnSpeed(),
                });
            } else {
                this.SendEvent<OnWeaponRecoilEvent>(new OnWeaponRecoilEvent()
                {
                    recoilVector = recoilProperty.GetRecoilVector(),
                    snappiness = recoilProperty.GetSnappiness(),
                    returnSpeed = recoilProperty.GetReturnSpeed(),
                });
            }
        }

        protected override string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName) {
            return "???";
        }
        
    }
}