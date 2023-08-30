using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Properties;

namespace Runtime.Weapons.Model.Base
{
    public interface IWeaponEntity : IResourceEntity, IHaveCustomProperties, IHaveTags {
        public IBaseDamage GetBaseDamage();
        public IAttackSpeed GetAttackSpeed();
        public IRange GetRange();
        public IAmmoSize GetAmmoSize();
        public IReloadSpeed GetReloadSpeed();
        public IBulletsPerShot GetBulletsPerShot();
        public ISpread GetSpread();
        public IRecoil GetRecoil();
        public IBulletSpeed GetBulletSpeed();
        public IChargeSpeed GetChargeSpeed();
    }
    
    public abstract class WeaponEntity<T> :  ResourceEntity<T>, IWeaponEntity where T : WeaponEntity<T>, new() {
        private IBaseDamage baseDamageProperty;
        private IAttackSpeed attackSpeedProperty;
        private IRange rangeProperty;
        private IAmmoSize ammoSizeProperty;
        private IReloadSpeed reloadSpeedProperty;
        private IBulletsPerShot bulletsPerShotProperty;
        private ISpread spreadProperty;
        private IRecoil recoilProperty;
        private IBulletSpeed bulletSpeedProperty;
        private IChargeSpeed chargeSpeedProperty;
        
        protected override ConfigTable GetConfigTable() {
            return ConfigDatas.Singleton.WeaponEntityConfigTable;
        }
        
        protected override void OnEntityStart() {
            base.OnEntityStart();
            baseDamageProperty = GetProperty<IBaseDamage>();
            attackSpeedProperty = GetProperty<IAttackSpeed>();
            rangeProperty = GetProperty<IRange>();
            ammoSizeProperty = GetProperty<IAmmoSize>();
            reloadSpeedProperty = GetProperty<IReloadSpeed>();
            bulletsPerShotProperty = GetProperty<IBulletsPerShot>();
            spreadProperty = GetProperty<ISpread>();
            recoilProperty = GetProperty<IRecoil>();
            bulletSpeedProperty = GetProperty<IBulletSpeed>();
            chargeSpeedProperty = GetProperty<IChargeSpeed>();
        }
        
        public override void OnDoRecycle() {
            SafeObjectPool<T>.Singleton.Recycle(this as T);
        }
        
        protected override void OnEntityRegisterAdditionalProperties() {
            RegisterInitialProperty<IBaseDamage>(new BaseDamage());
            RegisterInitialProperty<IAttackSpeed>(new AttackSpeed());
            RegisterInitialProperty<IRange>(new Range());
            RegisterInitialProperty<IAmmoSize>(new AmmoSize());
            RegisterInitialProperty<IReloadSpeed>(new ReloadSpeed());
            RegisterInitialProperty<IBulletsPerShot>(new BulletsPerShot());
            RegisterInitialProperty<ISpread>(new Spread());
            RegisterInitialProperty<IRecoil>(new Recoil());
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
        
        public IBulletSpeed GetBulletSpeed()
        {
            return bulletSpeedProperty;
        }
        
        public IChargeSpeed GetChargeSpeed()
        {
            return chargeSpeedProperty;
        }
    }
}