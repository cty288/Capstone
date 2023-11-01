using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IAdsFOV : IProperty<float>, ILoadFromConfigProperty { }
    public class AdsFOV : AbstractLoadFromConfigProperty<float>, IAdsFOV
    {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return new AdsFOVDefaultModifier();
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.ads_fov;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class AdsFOVDefaultModifier : PropertyDependencyModifierWithRarity<float>
    {
        protected override float OnModify(float propertyValue, int rarity)
        {
            return propertyValue;
        }
    }
}