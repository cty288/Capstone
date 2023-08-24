using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IBulletPerShot : IProperty<int>, ILoadFromConfigProperty { }
    public class BulletPerShot : AbstractLoadFromConfigProperty<int>, IBulletPerShot
    {
        protected override IPropertyDependencyModifier<int> GetDefautModifier() {
            return new BulletPerShotDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.bullet_per_shot;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class BulletPerShotDefaultModifier : PropertyDependencyModifier<int>
    {
        public override int OnModify(int propertyValue)
        {
            return propertyValue;
        }
    }
}