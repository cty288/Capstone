using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IAttackSpeed : IProperty<float>, ILoadFromConfigBuffedProperty { }
    
    public class AttackSpeed : AbstractLoadFromConfigBuffedProperty<float>, IAttackSpeed
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

        public override HashSet<BuffTag> BuffTags { get; } = new HashSet<BuffTag> {BuffTag.Weapon_AttackSpeed};
    }

    public class AttackSpeedDefaultModifier : PropertyDependencyModifierWithRarity<float>
    {
        protected override float OnModify(float propertyValue, int rarity) {
            return propertyValue;
        }
    }
}