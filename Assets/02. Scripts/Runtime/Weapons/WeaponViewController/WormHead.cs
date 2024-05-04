using System;
using System.Collections;
using System.Collections.Generic;
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
using UnityEngine.VFX;

namespace Runtime.Weapons
{
    public class WormHeadEntity : WeaponEntity<WormHeadEntity>
    {
        [field: ES3Serializable] public override string EntityName { get; set; } = "WormHead";
        
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

    
    public class WormHead : AbstractHitScanWeaponViewController<WormHeadEntity>
    {
        private GunAmmoVisual gunAmmoVisual;
        
        [Header("Debug")]
        [SerializeField] private string overrideName = "WormHead";
        
        protected override void OnEntityStart() {
            base.OnEntityStart();
        }

        protected override IHitDetector OnCreateHitDetector()
        {
            return new HitScan(this, CurrentFaction.Value, BulletVFXAll, fpsCamera);
        }

        protected override IEntity OnInitWeaponEntity(WeaponBuilder<WormHeadEntity> builder) {
            return builder.OverrideName(overrideName).FromConfig().Build();
        }

        #region Item Use

        public override void OnItemStartUse()
        {
            //semi-auto
            CheckShoot();
        }
        
        public override void OnItemUse()
        {
        }

        public override void OnItemStopUse() {}

        #endregion
        
        
        //TEMP CHANGE UNTIL ANIMATIONS ARE ADDED
        protected override void SetShootStatus(bool isShooting) {
            if (isShooting) {
                AudioSystem.Singleton.Play2DSound(shootSoundName, 1f);
                this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.Trigger,0));
            }
            else {
                this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.ResetTrigger,0));
            }
        }
        protected override IEnumerator ReloadAnimation() {
            ChangeReloadStatus(true);
            this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("ReloadSpeed", 
                AnimationEventType.Float,reloadAnimationLength/BoundEntity.GetReloadSpeed().RealValue));
            
            yield return new WaitForSeconds(BoundEntity.GetReloadSpeed().RealValue);
        }
        
        protected override void ShootEffects()
        {
            base.ShootEffects();
        }
        
    }
}
