using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCircleCollider2D;
using JetBrains.Annotations;
using MikroFramework;
using MikroFramework.BindableProperty;
using Runtime.Controls;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.Temporary.Player;
using Runtime.Temporary.Weapon;
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
    public class RustyPistolEntity : WeaponEntity<RustyPistolEntity>
    {
        [field: SerializeField] public override string EntityName { get; set; } = "RustyPistol";
        
        public override void OnRecycle()
        {
        }
        
        protected override string OnGetDescription(string defaultLocalizationKey)
        {
            return null;
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return null;
        }

        public override ResourceCategory GetResourceCategory() {
            return ResourceCategory.Weapon;
        }

        public override string OnGroundVCPrefabName { get; } = "RustyPistolOnGround";
    }

    public class RustyPistol : AbstractWeaponViewController<RustyPistolEntity>, IHitResponder
    {
        [Header("Auto Reload")]
        public bool autoReload = false;
        
        [Header("HitDetector Settings")] [SerializeField]
        [ReadOnly] private Camera cam;
        private HitScan hitScan;
        private HitDetectorInfo hitDetectorInfo;
        
        [SerializeField] private GameObject hitParticlePrefab;
        
        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction>(Faction.Friendly);

        
        // For IHitResponder.
        public int Damage => BoundEntity.GetBaseDamage().RealValue;
        private DPunkInputs.PlayerActions playerActions;

        protected override void Awake() {
            base.Awake();
            playerActions = ClientInput.Singleton.GetPlayerActions();
            cam = Camera.main;
        }
        
        protected override IEntity OnInitWeaponEntity(WeaponBuilder<RustyPistolEntity> builder) {
            return builder.FromConfig().Build();
        }
        
        protected override void OnBindEntityProperty() {}
        
        //=====
        public float lastShootTime = 0f;
        public float reloadStartTime = 0f;
        public bool isReloading = false;
        public bool isScopedIn = false;
        public int currentAmmo;
        public Transform gunPositionTransform;
        public Transform scopeInPositionTransform;
        
        // private ObjectPool<TrailRenderer> trailPool;
        public TrailRenderer trailRenderer;
        public ParticleSystem particleSystem;
        public LayerMask layer;
        
        //FOR CHARGE SPEED
        // private InputAction _holdAction;
        
        protected override void OnEntityStart()
        {
            //FOR CHARGE SPEED
            //TODO: Set shoot action hold duration when weapon is equipped.
            // _holdAction = playerActions.Shoot;
            // _holdAction.started += OnHoldActionStarted;
            
            currentAmmo = BoundEntity.GetAmmoSize().RealValue;
            
            hitScan = new HitScan(this, CurrentFaction.Value, trailRenderer);
            hitDetectorInfo = new HitDetectorInfo
            {
                camera = cam,
                layer = layer,
                launchPoint = particleSystem.transform,
                weapon = BoundEntity
            };
        }
            
        public void Shoot()
        {
            particleSystem.Play();
            
            hitScan.CheckHit(hitDetectorInfo);
        }
        
        public void Update()
        {
            //Shoot
            if (playerActions.Shoot.WasPerformedThisFrame() && !isReloading)
            {
                if (currentAmmo > 0 &&
                    Time.time > lastShootTime + BoundEntity.GetAttackSpeed().RealValue)
                {
                    lastShootTime = Time.time;
            
                    Shoot();
                    
                    currentAmmo--;

                    if (autoReload)
                    {
                        if (currentAmmo == 0)
                        {
                            if (isScopedIn)
                            {
                                StartCoroutine(ScopeOut(true));
                            }
                            else
                            {
                                isReloading = true;
                                reloadStartTime = Time.time;
                            }
                        }
                    } 
                }
            }
            
            //Reload
            if (playerActions.Reload.WasPerformedThisFrame() && !isReloading && currentAmmo < BoundEntity.GetAmmoSize().RealValue)
            {
                if (isScopedIn)
                {
                    StartCoroutine(ScopeOut(true));
                }
                else
                {
                    isReloading = true;
                    reloadStartTime = Time.time;
                }
            }

            if (isReloading && (Time.time > reloadStartTime + BoundEntity.GetReloadSpeed().RealValue))
            {
                currentAmmo = BoundEntity.GetAmmoSize().RealValue;
                StartCoroutine(ReloadAnimation());
            }
            
            // Scope
            if (playerActions.Scope.WasPerformedThisFrame() && !isReloading)
            {
                if (isScopedIn)
                {
                    StartCoroutine(ScopeOut());
                }
                else
                {                    
                    StartCoroutine(ScopeIn());
                }
            }
        }
        
        //FOR CHARGE SPEED
        // void OnDisable() {
        //     _holdAction.started -= OnHoldActionStarted;
        // }
        //
        // protected void OnHoldActionStarted(InputAction.CallbackContext context) {
        //     HoldInteraction holdInteraction = context.interaction as HoldInteraction;
        //     holdInteraction.duration = BoundEntity.GetChargeSpeed().RealValue.Value;
        //     Debug.Log(holdInteraction.duration);
        // }
        
        private IEnumerator ReloadAnimation()
        {
            float startTime = 0f;
            float dipTime = 0.25f;
            float riseTime = 0.5f;

            while ((dipTime - startTime) / dipTime > 0f)
            {
                transform.position = Vector3.Lerp(
                    gunPositionTransform.position,
                    new Vector3(gunPositionTransform.position.x, gunPositionTransform.position.y - 0.2f, gunPositionTransform.position.z),
                    (dipTime - startTime) / dipTime);
                
                startTime += Time.deltaTime;
                yield return null;
            }
            
            while (((riseTime - startTime) / riseTime) > 0f)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    gunPositionTransform.position,
                    (riseTime - startTime) / riseTime);
                
                startTime += Time.deltaTime;
                yield return null;
            }
            
            yield return null;
            isReloading = false;
        } 
        
        private IEnumerator ScopeIn()
        {
            float startTime = 0f;
            float amimationTime = 0.1f;

            while ((amimationTime - startTime) / amimationTime > 0f)
            {
                transform.position = Vector3.Lerp(
                    scopeInPositionTransform.position,
                    gunPositionTransform.position,
                    (amimationTime - startTime) / amimationTime);
                
                startTime += Time.deltaTime;
                yield return null;
            }
            
            yield return null;
            isScopedIn = true;
        } 
        
        private IEnumerator ScopeOut(bool reloadAfter = false)
        {
            float startTime = 0f;
            float amimationTime = 0.1f;

            while ((amimationTime - startTime) / amimationTime > 0f)
            {
                transform.position = Vector3.Lerp(
                    gunPositionTransform.position,
                    scopeInPositionTransform.position,
                    (amimationTime - startTime) / amimationTime);
                
                startTime += Time.deltaTime;
                yield return null;
            }
            
            yield return null;
            isScopedIn = false;
            
            if (reloadAfter)
            {
                isReloading = true;
                reloadStartTime = Time.time;
            }
        } 

        
        public bool CheckHit(HitData data)
        {
            return data.Hurtbox.Owner != gameObject;
        }
        
        public void HitResponse(HitData data)
        {
            Instantiate(hitParticlePrefab, data.HitPoint, Quaternion.identity);
            
            //TODO: Change to non-temporary class of player entity.
            PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
            // playerMovement.rb.AddForce(data.Recoil * -data.HitDirectionNormalized, ForceMode.Impulse);
        }
    }
}