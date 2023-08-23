using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IChargeSpeed : IProperty<float>, ILoadFromConfigProperty { }
    public class ChargeSpeed : AbstractLoadFromConfigProperty<float>, IChargeSpeed
    {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return new ChargeSpeedDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.charge_speed;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class ChargeSpeedDefaultModifier : PropertyDependencyModifier<float>
    {
        public override float OnModify(float propertyValue)
        {
            return propertyValue;
        }
    }
}