using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IRecoil : IProperty<float>, ILoadFromConfigProperty { }
    public class Recoil : AbstractLoadFromConfigProperty<float>, IRecoil
    {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return new RecoilDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.recoil;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class RecoilDefaultModifier : PropertyDependencyModifier<float>
    {
        public override float OnModify(float propertyValue)
        {
            return propertyValue;
        }
    }
}