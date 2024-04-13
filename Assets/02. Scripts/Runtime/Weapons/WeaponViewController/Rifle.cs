using System.Collections.Generic;
using MikroFramework.AudioKit;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.VFX;

namespace Runtime.Weapons
{
    public class RifleEntity : WeaponEntity<RifleEntity>
    {
        [field: ES3Serializable] public override string EntityName { get; set; } = "Rifle";

        public override bool Collectable => true;
        [field: ES3Serializable] public override int Width { get; } = 1;
        
        protected override string OnGetDescription(string defaultLocalizationKey) {
            return Localization.Get(defaultLocalizationKey);
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return null;
        }

        protected override void OnInitModifiers(int rarity) {

        }
        
        
        public override string OnGroundVCPrefabName => EntityName;
    }

    
    public class Rifle : AbstractHitScanWeaponViewController<RifleEntity>
    {
        private GunAmmoVisual gunAmmoVisual;
        
        [Header("Debug")]
        [SerializeField] private string overrideName = "Rifle";
        
        private bool isFullyAutomatic = false;
        
        protected override void OnEntityStart() {
            base.OnEntityStart();
            gunAmmoVisual = GetComponentInChildren<GunAmmoVisual>(true);
            gunAmmoVisual.Init(BoundEntity);
        }
        protected override IHitDetector OnCreateHitDetector()
        {
            return new HitScan(this, CurrentFaction.Value, BulletVFXAll, fpsCamera);
        }

        protected override IEntity OnInitWeaponEntity(WeaponBuilder<RifleEntity> builder) {
            return builder.OverrideName(overrideName).FromConfig().Build();
        }

        public override void OnItemUse()
        {
            
            if (isFullyAutomatic)
            {
                CheckShoot();
            }
        }
        
        public override void OnItemStartUse()
        {
            if (!isFullyAutomatic)
            {
                CheckShoot();
            }
        }
        
        public override void OnItemAltUse()
        {
            base.OnItemAltUse();
            isFullyAutomatic = !isFullyAutomatic;
            AudioSystem.Singleton.Play2DSound("gun_click");
        }
    }
}