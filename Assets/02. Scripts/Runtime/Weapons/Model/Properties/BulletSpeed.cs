using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IBulletSpeed : IProperty<float>, ILoadFromConfigProperty { }
    public class BulletSpeed : AbstractLoadFromConfigProperty<float>, IBulletSpeed
    {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return new BulletSpeedDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.bullet_speed;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class BulletSpeedDefaultModifier  : PropertyDependencyModifierWithRarity<float>
    {
        protected override float OnModify(float propertyValue, int rarity) {
            return propertyValue;
        }
    }
}