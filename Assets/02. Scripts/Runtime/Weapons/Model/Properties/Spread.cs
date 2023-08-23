using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface ISpread : IProperty<int>, ILoadFromConfigProperty { }
    public class Spread : AbstractLoadFromConfigProperty<int>, ISpread
    {
        protected override IPropertyDependencyModifier<int> GetDefautModifier() {
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

    public class SpreadDefaultModifier : PropertyDependencyModifier<int>
    {
        public override int OnModify(int propertyValue)
        {
            return propertyValue;
        }
    }
}