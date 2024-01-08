using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Runtime.DataFramework.Properties;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Weapons.Model.Properties
{
    public interface IBaseDamage : IBuffedProperty<Vector2Int>, ILoadFromConfigBuffedProperty { }
    public class BaseDamage : AbstractLoadFromConfigBuffedProperty<Vector2Int>, IBaseDamage
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

        public override HashSet<BuffTag> BuffTags { get; } = new HashSet<BuffTag>()
        {
            BuffTag.Weapon_BaseDamage,
        };
    }

    public class BaseDamageDefaultModifier : PropertyDependencyModifierWithRarity<Vector2Int>
    {
        protected override Vector2Int OnModify(Vector2Int propertyValue, int rarity)
        {
            return propertyValue;
        }
    }
}