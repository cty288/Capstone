using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IAttackSpeed : IProperty<float>, ILoadFromConfigProperty { }
    
    public class AttackSpeed : AbstractLoadFromConfigProperty<float>, IAttackSpeed
    {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return new AttackSpeedDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.attack_speed;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class AttackSpeedDefaultModifier : PropertyDependencyModifierWithRarity<float>
    {
        protected override float OnModify(float propertyValue, int rarity) {
            return propertyValue;
        }
    }
}