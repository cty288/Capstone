using Runtime.DataFramework.Properties;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Weapons.Model.Properties
{
    public interface IWeight : IProperty<float>, ILoadFromConfigProperty
    {
    }
    public class Weight : AbstractLoadFromConfigProperty<float>, IWeight
    {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return new WeightDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.weight;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }
    }

    public class WeightDefaultModifier : PropertyDependencyModifierWithRarity<float>
    {
        protected override float OnModify(float propertyValue, int rarity)
        {
            return propertyValue;
        }
    }
    
   
}