using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Runtime.DataFramework.Properties;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Weapons.Model.Properties
{
    public interface IScopeRecoil : IBuffedProperty<RecoilInfo>, ILoadFromConfigProperty
    {
        public Vector3 GetRecoilVector();
        public float GetSnappiness();
        public float GetReturnSpeed();
    }
    public class ScopeRecoil : AbstractLoadFromConfigBuffedProperty<RecoilInfo>, IScopeRecoil
    {
        public Vector3 GetRecoilVector()
        {
            return new Vector3(RealValue.Value.RecoilX, RealValue.Value.RecoilY, RealValue.Value.RecoilZ);
        }

        public float GetSnappiness()
        {
            return RealValue.Value.Snappiness;
        }

        public float GetReturnSpeed()
        {
            return RealValue.Value.ReturnSpeed;
        }
        
        protected override IPropertyDependencyModifier<RecoilInfo> GetDefautModifier() {
            return new ScopeRecoilDefaultModifier();
        }

        protected override PropertyName GetPropertyName()
        {
            return PropertyName.scope_recoil;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties()
        {
            return null;
        }

        public override HashSet<BuffTag> BuffTags { get; } = new HashSet<BuffTag> {BuffTag.Weapon_ScopeRecoil};
    }

    public class ScopeRecoilDefaultModifier : PropertyDependencyModifierWithRarity<RecoilInfo>
    {
        protected override RecoilInfo OnModify(RecoilInfo propertyValue, int rarity)
        {
            return propertyValue;
        }
    }
    
   
}