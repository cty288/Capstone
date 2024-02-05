using System;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityQuaternion;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using Polyglot;
using Runtime.Controls;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.Player;
using Runtime.Temporary.Weapon;
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Weapons
{
    public class RustyPistolEntity : WeaponEntity<RustyPistolEntity>
    {
        [field: ES3Serializable] public override string EntityName { get; set; } = "RustyPistol";
        
        [field: ES3Serializable] public override int Width { get; } = 1;
        
        protected override string OnGetDescription(string defaultLocalizationKey) {
            return Localization.Get(defaultLocalizationKey);
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return null;
        }
        public override bool Collectable  => true;
        protected override void OnInitModifiers(int rarity) {

        }
        
        
        public override string OnGroundVCPrefabName => EntityName;

       
    }

    
    public class RustyPistol : AbstractHitScanWeaponViewController<RustyPistolEntity>
    {
        private GunAmmoVisual gunAmmoVisual;
        
        [Header("Debug")]
        [SerializeField] private string overrideName = "RustyPistol";
        
        protected override void OnEntityStart() {
            base.OnEntityStart();
            gunAmmoVisual = GetComponentInChildren<GunAmmoVisual>(true);
            gunAmmoVisual.Init(BoundEntity);
        }

        protected override IHitDetector OnCreateHitDetector()
        {
            return new HitScan(this, CurrentFaction.Value, bulletVFX, fpsCamera);
        }

        protected override IEntity OnInitWeaponEntity(WeaponBuilder<RustyPistolEntity> builder) {
            return builder.OverrideName(overrideName).FromConfig().Build();
        }

        #region Item Use

        public override void OnItemStartUse()
        {
            // For semi-auto gun
            if (!isReloading) {
                if (BoundEntity.CurrentAmmo > 0 &&
                    Time.time > lastShootTime + BoundEntity.GetAttackSpeed().RealValue) {
                    lastShootTime = Time.time;
                    
                    SetShoot(true);
                    ShootEffects();

                    BoundEntity.CurrentAmmo.Value--;
                }
                
                if (autoReload && BoundEntity.CurrentAmmo <= 0)
                {
                    SetShoot(false);
                    ChangeReloadStatus(true);
                    StartCoroutine(ReloadAnimation());
                }
            }
        }
        
        public override void OnItemUse()
        {
        }

        public override void OnItemStopUse() {}

        #endregion
        
        
        protected override void ShootEffects()
        {
            base.ShootEffects();
        }
        
    }
}
