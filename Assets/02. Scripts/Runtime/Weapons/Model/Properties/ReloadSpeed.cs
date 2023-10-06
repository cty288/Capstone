using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IReloadSpeed : IProperty<float>, ILoadFromConfigProperty { }
    public class ReloadSpeed : AbstractLoadFromConfigProperty<float>, IReloadSpeed
    {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return new ReloadSpeedDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.reload_speed;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class ReloadSpeedDefaultModifier  : PropertyDependencyModifierWithRarity<float>
    {
        protected override float OnModify(float propertyValue, int rarity) {
            return propertyValue;
        }
    }
}