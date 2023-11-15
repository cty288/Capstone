using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCircleCollider2D;
using DG.Tweening;
using JetBrains.Annotations;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Polyglot;
using Runtime.Controls;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
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
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Runtime.Weapons
{
    public class TheSanctuaryEntity : WeaponEntity<TheSanctuaryEntity>
    {
        [field: SerializeField] public override string EntityName { get; set; } = "TheSanctuary";
        
        [field: ES3Serializable] public override int Width { get; } = 2;
        
        public override void OnRecycle()
        {
        }
        
        protected override string OnGetDescription(string defaultLocalizationKey) {
            return Localization.Get(defaultLocalizationKey);
        }

        protected override void OnInitModifiers(int rarity)
        {
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return null;
        }


        public override string OnGroundVCPrefabName { get; } = "TheSanctuary";

    }

    public class TheSanctuary : AbstractProjectileWeaponViewController<TheSanctuaryEntity>
    {
        // For Coroutine Animation [WILL BE REPLACED]
       // public GameObject model;
        public Transform gunPositionTransform;
        public Transform scopeInPositionTransform;

        public Transform bulletSpawnPos;
        public GameObject bulletPrefab;
        
        private SafeGameObjectPool pool;
        
        protected override void OnEntityStart()
        {
            base.OnEntityStart();
            
            playerModel = this.GetModel<IGamePlayerModel>();
           // BoundEntity.animLayerName = animLayerNameOverride;

           // animLayerNameOverride = "Revolver";
            // Debug.Log($"sanctuary camera pos: {hipFireCameraPosition}, {adsCameraPosition}");
            hipFireCameraPosition = hipFireCameraPositionOverride;
            adsCameraPosition = adsCameraPositionOverride;
        }
        
        protected override void Awake() {
            base.Awake();
            playerActions = ClientInput.Singleton.GetPlayerActions();
            cam = Camera.main;
            
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab, 20, 50);
            animationSMBManager = GetComponent<AnimationSMBManager>();
            animationSMBManager.Event.AddListener(OnAnimationEvent);
        }
        
        protected override IEntity OnInitWeaponEntity(WeaponBuilder<TheSanctuaryEntity> builder) {
            return builder.FromConfig().Build();
        }
        
        protected override void OnBindEntityProperty() {}
        
        public override void OnItemUse() {
            if (!isReloading) {
                if (BoundEntity.CurrentAmmo > 0 &&
                    Time.time > lastShootTime + BoundEntity.GetAttackSpeed().RealValue) {
                    lastShootTime = Time.time;
                    
                    SetShoot(true);

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

        public override void SetShoot(bool shouldShoot)
        {
            base.SetShoot(shouldShoot);
            if (shouldShoot) {
                Vector3 shootDir = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction;
            
                GameObject b = pool.Allocate();
                b.transform.position = bulletSpawnPos.position;
                b.transform.rotation = Quaternion.identity;
            
                b.GetComponent<Rigidbody>().velocity = shootDir * BoundEntity.GetBulletSpeed().RealValue;

                b.GetComponent<IBulletViewController>().Init(CurrentFaction.Value,
                    BoundEntity.GetRealDamageValue(),
                    gameObject, gameObject.GetComponent<ICanDealDamage>(), BoundEntity.GetRange().BaseValue);
            }
            
        }
        
        public override void OnItemScopePressed() {
            if (isReloading) {
                return;
            }
            if (IsScopedIn) {
                ChangeScopeStatus(false);
                fpsCamera.transform.DOLocalMove(hipFireCameraPosition, 0.167f);
            }
            else {
                ChangeScopeStatus(true);
                fpsCamera.transform.DOLocalMove(adsCameraPosition, 0.167f);
            }
        }

        public override void OnItemScopeReleased() {
            
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
                    }
                    
                    StartCoroutine(ReloadAnimation());
                }
                
            }
        }
        
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

        public override void OnRecycled() {
            base.OnRecycled();

        }

        protected override void OnReadyToRecycle() {
            base.OnReadyToRecycle();
            fpsCamera.transform.DOLocalMove(hipFireCameraPosition, 0.167f);
            ChangeScopeStatus(false);
            ChangeReloadStatus(false);
        }
    }
}
