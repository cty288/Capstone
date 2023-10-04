using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IBaseDamage : IProperty<int>, ILoadFromConfigProperty { }
    public class BaseDamage : AbstractLoadFromConfigProperty<int>, IBaseDamage
    {
        protected override IPropertyDependencyModifier<int> GetDefautModifier() {
            return new BaseDamageDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.base_damage;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class BaseDamageDefaultModifier : PropertyDependencyModifierWithRarity<int>
    {
        protected override int OnModify(int propertyValue, int rarity)
        {
            return propertyValue;
        }
    }
}