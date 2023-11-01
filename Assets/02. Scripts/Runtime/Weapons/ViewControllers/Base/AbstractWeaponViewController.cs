using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Player;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.Model.Properties;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Weapons.ViewControllers.Base
{
    public struct OnScopeUsedEvent
    {
        public bool isScopedIn;
    }
    
    public interface IWeaponViewController : IResourceViewController, ICanDealDamageViewController, ICanSendEvent {
        IWeaponEntity WeaponEntity { get; }
    }
    /// <summary>
    /// For both 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractWeaponViewController<T> : AbstractPickableInHandResourceViewController<T>, IWeaponViewController, IBelongToFaction, IHitResponder
        where T : class, IWeaponEntity, new() {
        public GameObject model;
        [Header("Auto Reload")]
        public bool autoReload = true;
        
        [Header("Layer Hit Mask")]
        public LayerMask layer;
        
        private IWeaponModel weaponModel;

        protected IHitDetector hitDetector;

        private bool _isScopedIn = false;
        protected bool IsScopedIn => _isScopedIn;
        
        // general references
        protected Camera cam;
        protected Camera fpsCamera;
        protected DPunkInputs.PlayerActions playerActions;
        protected IGamePlayerModel playerModel;
        public GameObject hitParticlePrefab;
        public VisualEffect hitVFXSystem;
        protected bool isHitVFX;
        protected CameraShaker cameraShaker;
        
        //status
        protected bool isScopedIn = false;
        protected bool isReloading = false;
        
        //timers
        protected float lastShootTime = 0f;
        protected float reloadTimer = 0f;
        
        //scoping
        protected Vector3 hipFireCameraPosition = new (0f, 0f, 0f);
        protected Vector3 adsCameraPosition = new (0f, 0f, 0f);

        protected ICanDealDamageViewController ownerVc;
        
        protected override void Awake() {
            base.Awake();
            weaponModel = this.GetModel<IWeaponModel>();
            playerModel = this.GetModel<IGamePlayerModel>();
            fpsCamera = mainCamera.GetUniversalAdditionalCameraData().cameraStack[0];

        }

        protected override void OnEntityStart() {
            base.OnEntityStart();
            hitDetector = OnCreateHitDetector();
            _isScopedIn = false;
            cameraShaker = FindObjectOfType<CameraShaker>();
        }

        public override void OnStartHold(GameObject ownerGameObject) {
            base.OnStartHold(ownerGameObject);
            if(ownerGameObject.TryGetComponent<ICanDealDamageViewController>(out var damageDealer)) {
                BoundEntity.CurrentFaction.Value = damageDealer.CanDealDamageEntity.CurrentFaction.Value;
                BoundEntity.SetRootDamageDealer(damageDealer.CanDealDamageEntity?.RootDamageDealer);
                ownerVc = damageDealer;
            }
        }

        public override void OnStopHold() {
            BoundEntity.CurrentFaction.Value = Faction.Neutral;
            BoundEntity.SetRootDamageDealer(null);
            base.OnStopHold();
            ChangeScopeStatus(false);
        }


        protected void ChangeScopeStatus(bool shouldScope) {
            bool previsScope = _isScopedIn;
            _isScopedIn = shouldScope;
            
            if (previsScope != _isScopedIn) {
                playerModel.GetPlayer().SetScopedIn(_isScopedIn);
                crossHairViewController?.OnScope(_isScopedIn);
                
                this.SendEvent<OnScopeUsedEvent>(new OnScopeUsedEvent()
                {
                    isScopedIn = _isScopedIn
                });
                
                AudioSystem.Singleton.Play2DSound("Pistol_Aim");
            }
           
            this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("ADS", AnimationEventType.Bool,!_isScopedIn ? 0 : 1));
        }

        protected void ChangeReloadStatus(bool shouldReload) {
            bool prevIsReloading = isReloading;
            isReloading = shouldReload;
            if (prevIsReloading != isReloading) {
                //crossHairViewController?.OnReload(isReloading);
            }
            this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Reload", AnimationEventType.Bool,isReloading ? 1 : 0));
        }
      

        public override void OnRecycled() {
            base.OnRecycled();
           
        }
        
        protected void SetShootStatus(bool isShooting) {
            if (isShooting) {
                AudioSystem.Singleton.Play2DSound("Pistol_Single_Shot", 1f);
                this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.Trigger,0));
            }
            else {
                this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("ShootEnd", AnimationEventType.Trigger,0));
            }
           
        }

        protected override void OnReadyToRecycle() {
            base.OnReadyToRecycle();
            _isScopedIn = false;
        }


        protected abstract IHitDetector OnCreateHitDetector();

       
        public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity) {
            if(weaponModel == null) {
                weaponModel = this.GetModel<IWeaponModel>();
            }
            WeaponBuilder<T> builder = weaponModel.GetWeaponBuilder<T>();
            if (setRarity) {
                builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), rarity);
            }

            return OnInitWeaponEntity(builder) as IResourceEntity;
        }

        protected abstract IEntity OnInitWeaponEntity(WeaponBuilder<T> builder);

        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);

        public void OnKillDamageable(IDamageable damageable) {
            BoundEntity.OnKillDamageable(damageable);
            ownerVc?.CanDealDamageEntity?.OnKillDamageable(damageable);
            crossHairViewController?.OnKill(damageable);
        }

        public void OnDealDamage(IDamageable damageable, int damage) {
            BoundEntity.OnDealDamage(damageable, damage);
            ownerVc?.CanDealDamageEntity?.OnDealDamage(damageable, damage);
            crossHairViewController?.OnHit(damageable, damage);
            Debug.Log(
                $"Weapon root owner {RootDamageDealer.RootDamageDealer.EntityName} deal damage to {damageable.EntityName} with damage {damage}");
        }

        public ICanDealDamageRootEntity RootDamageDealer => ownerVc?.CanDealDamageEntity?.RootDamageDealer;

        public int Damage => BoundEntity.GetRealDamageValue();
        public bool CheckHit(HitData data) {
            return data.Hurtbox.Owner != gameObject;
        }

        public abstract void HitResponse(HitData data);
        public IWeaponEntity WeaponEntity => BoundEntity;
        public ICanDealDamage CanDealDamageEntity => BoundEntity;
    }
}
