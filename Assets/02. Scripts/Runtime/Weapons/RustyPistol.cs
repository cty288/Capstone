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
        [field: SerializeField] public override string EntityName { get; set; } = "RustyPistol";
        
        [field: ES3Serializable] public override int Width { get; } = 1;
        
        public override void OnRecycle()
        {
        }
        
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

    
    public class RustyPistol : AbstractHitScanWeaponViewController<RustyPistolEntity>
    {
        private GunAmmoVisual gunAmmoVisual;
        // [SerializeField] private Animator animator;
        // [SerializeField] private float reloadAnimationLength;
        // private AnimationSMBManager animationSMBManager;
        
        [Header("Debug")]
        [SerializeField] private string overrideName = "RustyPistol";
        
        protected override void Awake() {
            base.Awake();
            playerActions = ClientInput.Singleton.GetPlayerActions();
            cam = Camera.main;
            gunAmmoVisual = GetComponentInChildren<GunAmmoVisual>(true);
            animationSMBManager = GetComponent<AnimationSMBManager>();
            animationSMBManager.Event.AddListener(OnAnimationEvent);
        }

        protected override void OnEntityStart() {
            base.OnEntityStart();
            gunAmmoVisual = GetComponentInChildren<GunAmmoVisual>(true);
            gunAmmoVisual.Init(BoundEntity);

            BoundEntity.animLayerName = animLayerNameOverride;

            //animLayerNameOverride = "Revolver";
            // Debug.Log($"sanctuary camera pos: {hipFireCameraPosition}, {adsCameraPosition}");
            hipFireCameraPosition = hipFireCameraPositionOverride;
            adsCameraPosition = adsCameraPositionOverride;
        }

        protected override IEntity OnInitWeaponEntity(WeaponBuilder<RustyPistolEntity> builder) {
            return builder.OverrideName(overrideName).FromConfig().Build();
        }
        
        protected override void OnBindEntityProperty() {}
        

        
        protected void OnAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case "ReloadStart":
                    AudioSystem.Singleton.Play2DSound("Pistol_Reload_Begin");
                    break;
                case "ReloadEnd":
                    ChangeReloadStatus(false);
                    AudioSystem.Singleton.Play2DSound("Pistol_Reload_Finish");
                    BoundEntity.Reload();
                    break;
                default:
                    break;
            }
        }
        public override void OnItemUse() {
            if (!isReloading) {
                if (BoundEntity.CurrentAmmo > 0 &&
                    Time.time > lastShootTime + BoundEntity.GetAttackSpeed().RealValue) {
                    lastShootTime = Time.time;
                    SetShoot(true);
                    animator.SetTrigger("Shoot");
                    
                    CameraShakeData shakeData = new CameraShakeData(
                        Mathf.Lerp(0.2f, 0.5f, IsScopedIn ? 1: 0),
                        0.2f,
                        3
                    );
                    cameraShaker.Shake(shakeData, CameraShakeBlendType.Maximum);
                    
                    BoundEntity.CurrentAmmo.Value--;
                }
                
                if (BoundEntity.CurrentAmmo == 0 && autoReload)
                {
                    SetShoot(false);
                    ChangeReloadStatus(true);
                    StartCoroutine(ReloadAnimation());
                }
            }
        }

        
        public override void OnItemScopePressed() {
            if (isReloading) {
                return;
            }
            if (IsScopedIn) {
                ChangeScopeStatus(false);
                //time is from animation
                fpsCamera.transform.DOLocalMove(hipFireCameraPosition, 0.167f);
            }
            else {
                ChangeScopeStatus(true);
                fpsCamera.transform.DOLocalMove(adsCameraPosition, 0.167f);
            }
        }


        protected override void Update()
        {
            base.Update();
            if (isHolding && !playerModel.IsPlayerDead())
            {
                //Reload
                if (playerActions.Reload.WasPerformedThisFrame() && !isReloading &&
                    BoundEntity.CurrentAmmo < BoundEntity.GetAmmoSize().RealValue)
                {
                    if (IsScopedIn)
                    {
                        ChangeScopeStatus(false);
                        //this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Reload",2));
                        //StartCoroutine(ScopeOut(true));
                    }
                    
                    //this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Reload",2));
                    StartCoroutine(ReloadAnimation());
                }
                
            }
        }

        public override void OnRecycled() {
            base.OnRecycled();
            fpsCamera.transform.DOLocalMove(hipFireCameraPosition, 0.167f);
            ChangeScopeStatus(false);
            ChangeReloadStatus(false);
        }
    }
}
