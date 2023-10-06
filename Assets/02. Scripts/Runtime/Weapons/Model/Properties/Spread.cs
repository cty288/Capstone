using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface ISpread : IProperty<float>, ILoadFromConfigProperty { }
    public class Spread : AbstractLoadFromConfigProperty<float>, ISpread
    {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return new SpreadDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.spread;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class SpreadDefaultModifier  : PropertyDependencyModifierWithRarity<float>
    {
        protected override float OnModify(float propertyValue, int rarity) {
            return propertyValue;
        }
    }
}