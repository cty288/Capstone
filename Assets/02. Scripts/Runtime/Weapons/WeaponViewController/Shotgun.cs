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
    public class ShotgunEntity : WeaponEntity<ShotgunEntity>
    {
        [field: ES3Serializable] public override string EntityName { get; set; } = "Shotgun";

        public override bool Collectable  => true;
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

    
    public class Shotgun : AbstractHitScanWeaponViewController<ShotgunEntity>
    {
        private GunAmmoVisual gunAmmoVisual;
        
        [Header("Debug")]
        [SerializeField] private string overrideName = "Shotgun";
        
        protected override void OnEntityStart() {
            base.OnEntityStart();
            gunAmmoVisual = GetComponentInChildren<GunAmmoVisual>(true);
            gunAmmoVisual.Init(BoundEntity);
        }
        
        protected override void SetSoundNames()
        {
            //basic sounds
            shootSoundName = "Shotgun_Single_Shot";
            reloadStartSoundName = "Shotgun_Reload_Begin";
            reloadFinishSoundName = "Shotgun_Reload_Finish";
        }
        
        protected override IHitDetector OnCreateHitDetector()
        {
            return new HitScan(this, CurrentFaction.Value, BulletVFXAll, fpsCamera);
        }

        protected override IEntity OnInitWeaponEntity(WeaponBuilder<ShotgunEntity> builder) {
            return builder.OverrideName(overrideName).FromConfig().Build();
        }

        protected override void Shoot()
        {
            base.Shoot();
            crossHairViewController?.OnShoot();
            BoundEntity.OnRecoil(IsScopedIn);
            //multiple shots per click
            for (int i = 0; i < BoundEntity.GetBulletsPerShot().RealValue; i++)
            {
                hitDetector.CheckHit(hitDetectorInfo, BoundEntity.GetRealDamageValue());
            }
        }

        public override void OnStartHold(GameObject ownerGameObject)
        {
            base.OnStartHold(ownerGameObject);
            AudioSystem.Singleton.Play2DSound("Shotgun_Equip");
        }
    }
}