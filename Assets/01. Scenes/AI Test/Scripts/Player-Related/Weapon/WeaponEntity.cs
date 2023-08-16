using MikroFramework.BindableProperty;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.Utilities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using MikroFramework.Pool;

namespace Runtime.DataFramework.Entities
{
    public interface IWeaponEntity : IEntity, IHaveCustomProperties
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

    public abstract class WeaponEntity<T> : AbstractHaveCustomPropertiesEntity, IWeaponEntity
    // , IHaveTags 
    where T : WeaponEntity<T>, new()
    {
        protected override void OnEntityRegisterProperties()
        {
            //TODO: CREATE WEAPON PROPERTIES
            RegisterInitialProperty(new BaseDamage());

            OnWeaponRegisterProperties();
        }

        protected abstract void OnWeaponRegisterProperties();

        public BindableProperty<int> GetBaseDamage()
        {
            return GetProperty<BaseDamage>().RealValue;
        }

        public override void OnDoRecycle()
        {
            SafeObjectPool<T>.Singleton.Recycle(this as T);
        }

    }
}