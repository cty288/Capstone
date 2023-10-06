using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IRange : IProperty<float>, ILoadFromConfigProperty { }
    public class Range : AbstractLoadFromConfigProperty<float>, IRange
    {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return new RangeDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.range;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class RangeDefaultModifier : PropertyDependencyModifierWithRarity<float>
    {
        protected override float OnModify(float propertyValue, int rarity) {
            return propertyValue;
        }
    }
}