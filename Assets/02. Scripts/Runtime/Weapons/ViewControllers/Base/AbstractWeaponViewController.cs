using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.BindableProperty;
using Runtime.Controls;
using DG.Tweening;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Player;
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Commands;
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
    
    [Serializable]
    public struct CameraPlacementData
    {
        public Vector3 hipFireCameraPosition;
        public Vector3 hipFireCameraRotation;

        public Vector3 adsCameraPosition;
        public Vector3 adsCameraRotation;
    }
    
    public interface IWeaponViewController : IResourceViewController,  IPickableResourceViewController, IInHandResourceViewController {
        IWeaponEntity WeaponEntity { get; }
        IEntity IEntityViewController.Entity => WeaponEntity;
        
        GameObject gameObject { get; }
    }
    
    public interface IWeaponVFX
    {
        public VisualEffect HitVFX { get; }

        public void SetVFX(VisualEffect vfx);
        public void ResetVFX();
    }
    
    /// <summary>
    /// For both 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractWeaponViewController<T> : AbstractPickableInHandResourceViewController<T>, IWeaponViewController, IWeaponVFX, IBelongToFaction, IHitResponder
        where T : class, IWeaponEntity, new() {
        public GameObject model;
        [Header("Gun Options")]
        public bool autoReload = true;
        public bool canScope = true;
        
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
        //public GameObject hitParticlePrefab;
        public VisualEffect hitVFXSystem;
        protected VisualEffect originalHitVFXSystem;
        protected bool isHitVFX;
        protected CameraShaker cameraShaker;
        [SerializeField] protected Animator animator;
        [SerializeField] protected float reloadAnimationLength;
        protected AnimationSMBManager animationSMBManager;

        public VisualEffect HitVFX => hitVFXSystem;

        //timers & status
        //protected bool isLocked = false;
        protected bool isReloading = false;
        protected float lastShootTime = 0f;
        protected float reloadTimer = 0f;
        
        //scoping
        [SerializeField] protected CameraPlacementData cameraPlacementData;

        //protected ICanDealDamageViewController ownerVc;
        public IWeaponEntity WeaponEntity => BoundEntity;
        public ICanDealDamage CanDealDamageEntity => BoundEntity;
        /*public ICanDealDamageRootEntity RootDamageDealer => ownerVc?.CanDealDamageEntity?.RootDamageDealer;
        public ICanDealDamageRootViewController RootViewController => ownerVc?.CanDealDamageEntity?.RootViewController;*/

        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);

        public int Damage => BoundEntity.GetRealDamageValue();
        
        private IBuffSystem buffSystem;
        private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
        private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;

        protected string shootSoundName;
        protected string reloadStartSoundName;
        protected string reloadFinishSoundName;
        protected float adsChangeDuration = 0.167f; //time is from arms animation
        
        #region Initialization
        protected override void Awake() {
            base.Awake();
            weaponModel = this.GetModel<IWeaponModel>();
            playerModel = this.GetModel<IGamePlayerModel>();
            fpsCamera = mainCamera.GetUniversalAdditionalCameraData().cameraStack[0];
            buffSystem = this.GetSystem<IBuffSystem>();
            cam = Camera.main;
            playerActions = ClientInput.Singleton.GetPlayerActions();
            animationSMBManager = GetComponent<AnimationSMBManager>();
            animationSMBManager.Event.AddListener(OnAnimationEvent);

            originalHitVFXSystem = hitVFXSystem;
            
            SetSoundNames();
        }

        protected virtual void SetSoundNames()
        {
            //basic sounds
            shootSoundName = "Pistol_Single_Shot";
            reloadStartSoundName = "Pistol_Reload_Begin";
            reloadFinishSoundName = "Pistol_Reload_Finish";
        }
        
        public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity,
            bool addToModelWhenBuilt = true) {
            if(weaponModel == null) {
                weaponModel = this.GetModel<IWeaponModel>();
            }

            WeaponBuilder<T> builder = weaponModel.GetWeaponBuilder<T>(addToModelWhenBuilt);
            if (setRarity) {
                builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), rarity);
            }

            return OnInitWeaponEntity(builder) as IResourceEntity;
        }
        
        protected abstract IEntity OnInitWeaponEntity(WeaponBuilder<T> builder);
        
        protected override void OnEntityStart() {
            base.OnEntityStart();
            _isScopedIn = false;
            cameraShaker = FindObjectOfType<CameraShaker>();
            WeaponEntity.SetBoundViewController(this);
        }

       
        protected override void OnBindEntityProperty() {}
        

        #endregion
        
        protected override void Update()
        {
            base.Update();
            WeaponUpdate();
        }

        protected virtual void WeaponUpdate()
        {
            if (isHolding && !playerModel.IsPlayerDead())
            {
                //Reload
                if (playerActions.Reload.WasPerformedThisFrame() && !isReloading && !WeaponEntity.IsLocked &&
                    BoundEntity.CurrentAmmo < BoundEntity.GetAmmoSize().RealValue)
                {
                    if (IsScopedIn)
                    {
                        ChangeScopeStatus(false);
                        fpsCamera.transform.DOLocalMove(cameraPlacementData.hipFireCameraPosition, adsChangeDuration);
                        fpsCamera.transform.DOLocalRotate(cameraPlacementData.hipFireCameraRotation, adsChangeDuration);
                    }
                    
                    StartCoroutine(ReloadAnimation());
                }
                
                if(playerActions.SprintHold.WasPerformedThisFrame() && IsScopedIn)
                {
                    ChangeScopeStatus(false);
                    fpsCamera.transform.DOLocalMove(cameraPlacementData.hipFireCameraPosition, adsChangeDuration);
                    fpsCamera.transform.DOLocalRotate(cameraPlacementData.hipFireCameraRotation, adsChangeDuration);
                }
            }
        }


        
        #region Animation
        protected virtual void OnAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case "ReloadStart":
                    OnReloadAnimationStart();
                    break;
                case "ReloadEnd":
                    OnReloadAnimationEnd();
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnReloadAnimationStart()
        {
            AudioSystem.Singleton.Play2DSound(reloadStartSoundName);
        }
        
        protected virtual IEnumerator ReloadAnimation() {
            ChangeReloadStatus(true);
            this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("ReloadSpeed", 
                AnimationEventType.Float,reloadAnimationLength/BoundEntity.GetReloadSpeed().RealValue));
            animator.SetFloat("ReloadSpeed",reloadAnimationLength/BoundEntity.GetReloadSpeed().RealValue);
            animator.SetTrigger("Reload");
            
            yield return new WaitForSeconds(BoundEntity.GetReloadSpeed().RealValue);
        }

        protected virtual void OnReloadAnimationEnd()
        {
            ChangeReloadStatus(false);
            AudioSystem.Singleton.Play2DSound(reloadFinishSoundName);
            BoundEntity.Reload();
        }
        #endregion

        #region Holding
        public override void OnStartHold(GameObject ownerGameObject) {
            base.OnStartHold(ownerGameObject);
            /*if(ownerGameObject.TryGetComponent<ICanDealDamage>(out var damageDealer)) {
                BoundEntity.SetOwner(damageDealer);
            }*/
            
            fpsCamera.transform.DOLocalMove(cameraPlacementData.hipFireCameraPosition, adsChangeDuration);
            fpsCamera.transform.DOLocalRotate(cameraPlacementData.hipFireCameraRotation, adsChangeDuration);
            
            if(BoundEntity.CurrentAmmo == 0 && autoReload && !WeaponEntity.IsLocked) {
                if (IsScopedIn)
                {
                    ChangeScopeStatus(false);
                    fpsCamera.transform.DOLocalMove(cameraPlacementData.hipFireCameraPosition, adsChangeDuration);
                    fpsCamera.transform.DOLocalRotate(cameraPlacementData.hipFireCameraRotation, adsChangeDuration);
                }
                
                StartCoroutine(ReloadAnimation());
            }
        }

        
        public override void OnStopHold() {
            BoundEntity.CurrentFaction.Value = Faction.Neutral;
           // BoundEntity.SetOwner(null);
            base.OnStopHold();
            ChangeScopeStatus(false);
            fpsCamera.transform.DOLocalMove(cameraPlacementData.hipFireCameraPosition, adsChangeDuration);
            fpsCamera.transform.DOLocalRotate(cameraPlacementData.hipFireCameraRotation, adsChangeDuration);
        }
        #endregion

        #region Scope/Reload Status
        protected virtual void ChangeScopeStatus(bool shouldScope) {
            bool previsScope = _isScopedIn;
            _isScopedIn = shouldScope;
            
            if (previsScope != _isScopedIn) {
                playerModel.GetPlayer().SetScopedIn(_isScopedIn);
                crossHairViewController?.OnScope(_isScopedIn);

                this.SendCommand(ScopeCommand.Allocate(_isScopedIn));
                
                AudioSystem.Singleton.Play2DSound("Pistol_Aim");
            }
           
            this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("ADS", AnimationEventType.Bool,!_isScopedIn ? 0 : 1));
        }

        protected void ChangeReloadStatus(bool shouldReload) {
            if (!canScope)
                return;
            
            bool prevIsReloading = isReloading;
            isReloading = shouldReload;
            if (prevIsReloading != isReloading) {
                //crossHairViewController?.OnReload(isReloading);
            }

            if (shouldReload) {
                this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Reload", AnimationEventType.Trigger, 0));
            }
        }
        #endregion
        
        #region Shooting
        
        protected void SetShootStatus(bool isShooting) {
            if (isShooting) {
                AudioSystem.Singleton.Play2DSound(shootSoundName, 1f);
                this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.Trigger,0));
                animator.SetTrigger("Shoot");
            }
            else {
                this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.ResetTrigger,0));
            }
        }
        
        public virtual void SetShoot(bool shouldShoot) {
            if (shouldShoot) {
                Shoot();
            }
            SetShootStatus(shouldShoot);
        }

        protected virtual void Shoot()
        {
            BoundEntity.OnRecoil(IsScopedIn);
        }

        protected virtual void ShootEffects()
        {
            CameraShakeData shakeData = new CameraShakeData(
                Mathf.Lerp(0.15f, 0.35f, IsScopedIn ? 1: 0),
                0.2f,
                3
            );
            cameraShaker.Shake(shakeData, CameraShakeBlendType.Maximum);
        }
        #endregion

        #region Item Use
        protected override void OnStartAbsorb() {}

        public override void OnItemStartUse() {}
        
        public override void OnItemUse()
        {
            // fully-automatic gun
            CheckShoot();
        }

        //abstracted for modularity
        protected void CheckShoot()
        {
            if (!isReloading) {
                if (BoundEntity.CurrentAmmo > 0 && !WeaponEntity.IsLocked &&
                    Time.time > lastShootTime + BoundEntity.GetAttackSpeed().RealValue) {
                    lastShootTime = Time.time;
                    
                    SetShoot(true);
                    ShootEffects();

                    BoundEntity.ShootUseAmmo(1);
                }
                
                if (autoReload && BoundEntity.CurrentAmmo <= 0 && !WeaponEntity.IsLocked)
                {
                    SetShoot(false);
                    
                    if (IsScopedIn)
                    {
                        ChangeScopeStatus(false);
                        fpsCamera.transform.DOLocalMove(cameraPlacementData.hipFireCameraPosition, adsChangeDuration);
                        fpsCamera.transform.DOLocalRotate(cameraPlacementData.hipFireCameraRotation, adsChangeDuration);
                    }
                    
                    StartCoroutine(ReloadAnimation());
                }
            }
        }

        public override void OnItemStopUse() {}
        
        public override void OnItemAltUse() { } 
        
        public override void OnItemScopePressed() {
            if (isReloading || playerModel.IsPlayerSprinting()) {
                return;
            }
            
            if (IsScopedIn) {
                ChangeScopeStatus(false);
                fpsCamera.transform.DOLocalMove(cameraPlacementData.hipFireCameraPosition, adsChangeDuration);
                fpsCamera.transform.DOLocalRotate(cameraPlacementData.hipFireCameraRotation, adsChangeDuration);
            }
            else {
                ChangeScopeStatus(true);
                fpsCamera.transform.DOLocalMove(cameraPlacementData.adsCameraPosition, adsChangeDuration);
                fpsCamera.transform.DOLocalRotate(cameraPlacementData.adsCameraRotation, adsChangeDuration);
            }
        }
        
        public override void OnItemScopeReleased() {
            
        }
        #endregion

        #region Damage and Hit Response
        public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
            //BoundEntity.OnKillDamageable(damageable);
            //ownerVc?.CanDealDamageEntity?.OnKillDamageable(damageable);
            crossHairViewController?.OnKill(damageable);
        }

        public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
            //BoundEntity.OnDealDamage(damageable, damage);
            //ownerVc?.CanDealDamageEntity?.OnDealDamage(damageable, damage);
            crossHairViewController?.OnHit(damageable, damage);
            /*Debug.Log(
                $"Weapon root owner {ownerVc?.CanDealDamageEntity} deal damage to {damageable.EntityName} with damage {damage}");*/
        }

        public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

        Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
            get => _onDealDamageCallback;
            set => _onDealDamageCallback = value;
        }

        Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
            get => _onKillDamageableCallback;
            set => _onKillDamageableCallback = value;
        }

        public ICanDealDamage ParentDamageDealer => BoundEntity;

        public virtual bool CheckHit(HitData data) {
            return data.Hurtbox.Owner != gameObject;
        }

        public abstract void HitResponse(HitData data);
        public HitData OnModifyHitData(HitData data) {
            if(BoundEntity == null) {
                return data;
            }
            return BoundEntity.OnModifyHitData(data);
        }

        #endregion

        #region Recycling
        protected override void OnReadyToRecycle() {
            base.OnReadyToRecycle();
            ChangeScopeStatus(false);
        }
        
        public override void OnRecycled() {
            WeaponEntity.SetBoundViewController(null);
            base.OnRecycled();
            fpsCamera.transform.DOLocalMove(cameraPlacementData.hipFireCameraPosition, adsChangeDuration);
            fpsCamera.transform.DOLocalRotate(cameraPlacementData.hipFireCameraRotation, adsChangeDuration);
            ChangeScopeStatus(false);
            ChangeReloadStatus(false);
            OnModifyDamageCountCallbackList.Clear();
            _onDealDamageCallback = null;
            _onKillDamageableCallback = null;
        }
        #endregion

        #region VFX

        public void SetVFX(VisualEffect vfx)
        {
            var t = vfx.transform;
            t.parent = hitVFXSystem.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            
            hitVFXSystem = vfx;
        }

        public void ResetVFX()
        {
            hitVFXSystem = originalHitVFXSystem;
        }

        #endregion
    }
}
