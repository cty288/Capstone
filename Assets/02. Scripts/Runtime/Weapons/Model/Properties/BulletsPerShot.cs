using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IBulletsPerShot : IProperty<int>, ILoadFromConfigProperty { }
    public class BulletsPerShot : AbstractLoadFromConfigProperty<int>, IBulletsPerShot
    {
        protected override IPropertyDependencyModifier<int> GetDefautModifier() {
            return new BulletsPerShotDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.bullets_per_shot;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class BulletsPerShotDefaultModifier : PropertyDependencyModifier<int>
    {
        public override int OnModify(int propertyValue)
        {
            return propertyValue;
        }
    }
}