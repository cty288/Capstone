using Runtime.DataFramework.Properties;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Weapons.Model.Properties
{
    public interface IBaseDamage : IProperty<Vector2Int>, ILoadFromConfigProperty { }
    public class BaseDamage : AbstractLoadFromConfigProperty<Vector2Int>, IBaseDamage
    {
        protected override IPropertyDependencyModifier<Vector2Int> GetDefautModifier() {
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

    public class BaseDamageDefaultModifier : PropertyDependencyModifierWithRarity<Vector2Int>
    {
        protected override Vector2Int OnModify(Vector2Int propertyValue, int rarity)
        {
            return propertyValue;
        }
    }
}