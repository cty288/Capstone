﻿using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Weapons
{
    public class SubMachineGunEntity : WeaponEntity<SubMachineGunEntity>
    {
        [field: ES3Serializable] public override string EntityName { get; set; } = "SubMachineGun";
        
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

    
    public class SubMachineGun : AbstractHitScanWeaponViewController<SubMachineGunEntity>
    {
        private GunAmmoVisual gunAmmoVisual;
        
        [Header("Debug")]
        [SerializeField] private string overrideName = "SubMachineGun";
        
        protected override void OnEntityStart() {
            base.OnEntityStart();
            gunAmmoVisual = GetComponentInChildren<GunAmmoVisual>(true);
            gunAmmoVisual.Init(BoundEntity);
        }
        protected override IHitDetector OnCreateHitDetector()
        {
            return new HitScan(this, CurrentFaction.Value, bulletVFX, fpsCamera);
        }

        protected override IEntity OnInitWeaponEntity(WeaponBuilder<SubMachineGunEntity> builder) {
            return builder.OverrideName(overrideName).FromConfig().Build();
        }
        
        protected override void ShootEffects()
        {
            CameraShakeData shakeData = new CameraShakeData(
                Mathf.Lerp(0.2f, 0.5f, IsScopedIn ? 1: 0),
                0.2f,
                3
            );
            cameraShaker.Shake(shakeData, CameraShakeBlendType.Maximum);
        }
    }
}