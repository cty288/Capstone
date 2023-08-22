using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Properties;

namespace Runtime.Weapons.Model.Base
{
    public interface IWeaponEntity : IEntity, IHaveCustomProperties, IHaveTags {
        public IBaseDamage GetBaseDamage();
        public IAttackSpeed GetAttackSpeed();
        // public BindableProperty<float> GetRange();
        // public BindableProperty<int> GetMagazineSize();
        // public BindableProperty<float> GetReloadTime();
        // public BindableProperty<int> GetBulletPerShot();
        // public BindableProperty<int> GetSpread();
        // public BindableProperty<float> GetRecoil();
        // public BindableProperty<float> GetChargeTime();
        // public BindableProperty<float> GetBulletSpeed();
    }
    
    public abstract class WeaponEntity<T> :  AbstractBasicEntity, IWeaponEntity where T : WeaponEntity<T>, new() {
        private IBaseDamage baseDamageProperty;
        private IAttackSpeed attackSpeedProperty;
        
        protected override ConfigTable GetConfigTable() {
            return ConfigDatas.Singleton.WeaponEntityConfigTable;
        }
        
        protected override void OnEntityStart() {
            base.OnEntityStart();
            baseDamageProperty = GetProperty<IBaseDamage>();
        }
        
        public override void OnDoRecycle() {
            SafeObjectPool<T>.Singleton.Recycle(this as T);
        }
        
        protected override void OnEntityRegisterAdditionalProperties() {
            RegisterInitialProperty<IBaseDamage>(new BaseDamage());
        }
        
        public IBaseDamage GetBaseDamage()
        {
            return baseDamageProperty;
        }
        
        public IAttackSpeed GetAttackSpeed()
        {
            return attackSpeedProperty;
        }
    }
}