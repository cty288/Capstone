using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Runtime.DataFramework.Properties;
using Runtime.Weapons.Model.Base;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Weapons.Model.Properties
{
    [Serializable]
    public struct RecoilInfo
    {
        public float RecoilX; 
        public float RecoilY;
        public float RecoilZ;
        public float Snappiness;
        public float ReturnSpeed;

        public RecoilInfo(float recoilX, float recoilY, float recoilZ, float snappiness, float returnSpeed)
        {
            RecoilX = recoilX;
            RecoilY = recoilY;
            RecoilZ = recoilZ;
            Snappiness = snappiness;
            ReturnSpeed = returnSpeed;
        }
        
        //operator +
        public static RecoilInfo operator +(RecoilInfo a, RecoilInfo b) {
            return new RecoilInfo(a.RecoilX + b.RecoilX, a.RecoilY + b.RecoilY, a.RecoilZ + b.RecoilZ, a.Snappiness + b.Snappiness, a.ReturnSpeed + b.ReturnSpeed);
        }
        
        //operator -
        public static RecoilInfo operator -(RecoilInfo a, RecoilInfo b) {
            return new RecoilInfo(a.RecoilX - b.RecoilX, a.RecoilY - b.RecoilY, a.RecoilZ - b.RecoilZ, a.Snappiness - b.Snappiness, a.ReturnSpeed - b.ReturnSpeed);
        }
    }

    public interface IRecoil : IBuffedProperty<RecoilInfo>, ILoadFromConfigProperty
    {
        public Vector3 GetRecoilVector();
        public float GetSnappiness();
        public float GetReturnSpeed();
    }
    public class Recoil : AbstractLoadFromConfigBuffedProperty<RecoilInfo>, IRecoil
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

        public override HashSet<BuffTag> BuffTags { get; } = new HashSet<BuffTag> {BuffTag.Weapon_HipFireRecoil};
    }

    public class RecoilDefaultModifier : PropertyDependencyModifierWithRarity<RecoilInfo>
    {
        protected override RecoilInfo OnModify(RecoilInfo propertyValue, int rarity)
        {
            return propertyValue;
        }
    }
    
   
}