using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Runtime.DataFramework.Properties;

namespace Runtime.Weapons.Model.Properties
{
    public interface IAmmoSize : IBuffedProperty<int>, ILoadFromConfigBuffedProperty { }
    public class AmmoSize : AbstractLoadFromConfigBuffedProperty<int>, IAmmoSize
    {
        protected override IPropertyDependencyModifier<int> GetDefautModifier() {
            return new AmmoSizeDefaultModifier();
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.ammo_size;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }

        public override HashSet<BuffTag> BuffTags { get; } = new HashSet<BuffTag>() {
            BuffTag.Weapon_AmmoSize,
        };
    }

    public class AmmoSizeDefaultModifier : PropertyDependencyModifierWithRarity<int>
    {
        protected override int OnModify(int propertyValue, int rarity)
        {
            return propertyValue;
        }
    }
}