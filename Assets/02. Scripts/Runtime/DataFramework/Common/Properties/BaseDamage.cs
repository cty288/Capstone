namespace Runtime.DataFramework.Properties
{
    public interface IBaseDamageProperty : IProperty<int>, ILoadFromConfigProperty { }
    public class BaseDamage : AbstractLoadFromConfigProperty<int>, IBaseDamageProperty
    {
        public override int OnSetBaseValueFromConfig(dynamic value)
        {
            return value;
        }

        protected override IPropertyDependencyModifier<int> GetDefautModifier()
        {
            return new BaseDamageDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.base_damage;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            throw new System.NotImplementedException();
        }
    }

    public class BaseDamageDefaultModifier : PropertyDependencyModifier<int>
    {
        public override int OnModify(int propertyValue)
        {
            return propertyValue;
        }
    }
}