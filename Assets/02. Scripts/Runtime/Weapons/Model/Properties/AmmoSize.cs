using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IAmmoSize : IProperty<int>, ILoadFromConfigProperty { }
    public class AmmoSize : AbstractLoadFromConfigProperty<int>, IAmmoSize
    {
        protected override IPropertyDependencyModifier<int> GetDefautModifier() {
            return new AmmoSizeDefaultModifier();
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.ammo_size;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class AmmoSizeDefaultModifier : PropertyDependencyModifierWithRarity<int>
    {
        protected override int OnModify(int propertyValue, int rarity)
        {
            return propertyValue;
        }
    }
}