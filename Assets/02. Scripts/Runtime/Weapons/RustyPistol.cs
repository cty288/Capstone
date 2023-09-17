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


        public override string OnGroundVCPrefabName { get; } = "RustyPistol";
    }

    public class RustyPistol : AbstractWeaponViewController<RustyPistolEntity>, IHitResponder
    {
        [Header("Auto Reload")]
        public bool autoReload = false;
        
        [Header("HitDetector Settings")] [SerializeField]
        [ReadOnly] private Camera cam;
        private HitDetectorInfo hitDetectorInfo;
        
        [SerializeField] private GameObject hitParticlePrefab;
        
        // For IHitResponder.
        
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
        public float reloadTimer = 0f;
        public bool isReloading = false;
        public bool isScopedIn = false;
        public GameObject model;
        public Transform gunPositionTransform;
        public Transform scopeInPositionTransform;
        
        public TrailRenderer trailRenderer;
        public LayerMask layer;
        
        //FOR CHARGE SPEED
        // private InputAction _holdAction;

        public GunRecoil recoilScript;
        
        protected override void OnEntityStart()
        {
            //FOR CHARGE SPEED
            //TODO: Set shoot action hold duration when weapon is equipped.
            // _holdAction = playerActions.Shoot;
            // _holdAction.started += OnHoldActionStarted;
            
            base.OnEntityStart();
            
            hitDetectorInfo = new HitDetectorInfo
            {
                camera = cam,
                layer = layer,
                launchPoint = trailRenderer.transform,
                weapon = BoundEntity
            };
        }

        protected override IHitDetector OnCreateHitDetector() {
            return new HitScan(this, CurrentFaction.Value, trailRenderer);
        }

        protected override void OnStartAbsorb() {
           
        }
        
        public void Shoot()
        {
            // particleSystem.Play();
            
            hitDetector.CheckHit(hitDetectorInfo);
            BoundEntity.OnRecoil();
        }
        
        public void Update()
        {
            if (isHolding)
            {
                //Shoot
                if (playerActions.Shoot.IsPressed() && !isReloading)
                {
                    if (BoundEntity.CurrentAmmo > 0 &&
                        Time.time > lastShootTime + BoundEntity.GetAttackSpeed().RealValue)
                    {
                        lastShootTime = Time.time;

                        Shoot();

                        BoundEntity.CurrentAmmo--;

                        if (autoReload)
                        {
                            if (BoundEntity.CurrentAmmo == 0)
                            {
                                if (isScopedIn)
                                {
                                    StartCoroutine(ScopeOut(true));
                                }
                                else
                                {
                                    isReloading = true;
                                }
                            }
                        }
                    }
                }

                //Reload
                if (playerActions.Reload.WasPerformedThisFrame() && !isReloading &&
                    BoundEntity.CurrentAmmo < BoundEntity.GetAmmoSize().RealValue)
                {
                    if (isScopedIn)
                    {
                        StartCoroutine(ScopeOut(true));
                    }
                    else
                    {
                        isReloading = true;
                    }
                }

                if (isReloading && (reloadTimer >= BoundEntity.GetReloadSpeed().RealValue))
                {
                    BoundEntity.Reload();
                    StartCoroutine(ReloadAnimation());
                    reloadTimer = 0f;
                }
                else if (isReloading)
                {
                    reloadTimer += Time.deltaTime;
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

            Vector3 lowerPosition = new Vector3(gunPositionTransform.position.x,
                gunPositionTransform.position.y - 0.2f, gunPositionTransform.position.z);
            
            while ((dipTime - startTime) / dipTime > 0f)
            {
                model.transform.position = Vector3.Lerp(
                    gunPositionTransform.position,
                    lowerPosition,
                    (dipTime - startTime) / dipTime);
                
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
                model.transform.position = Vector3.Lerp(
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
                model.transform.position = Vector3.Lerp(
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
            }
        }

        public bool CheckHit(HitData data)
        {
            return data.Hurtbox.Owner != gameObject;
        }
        
        public override void HitResponse(HitData data) {
            Instantiate(hitParticlePrefab, data.HitPoint, Quaternion.identity);
            // float positionMultiplier = 1f;
            // float spawnX = data.HitPoint.x - data.HitNormal.x * positionMultiplier;
            // float spawnY = data.HitPoint.y - data.HitNormal.y * positionMultiplier;
            // float spawnZ = data.HitPoint.z - data.HitNormal.z * positionMultiplier;
            // Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);
            //
            // GameObject bulletHole = bulletHolesPool.Get();
            // bulletHole.transform.position = spawnPosition;
            // bulletHole.transform.rotation = Quaternion.LookRotation(data.HitNormal);
        }
    }
}
