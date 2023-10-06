using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCircleCollider2D;
using JetBrains.Annotations;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Polyglot;
using Runtime.Controls;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.Player;
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
        // For Coroutine Animation [WILL BE REPLACED]
        public GameObject model;
        public Transform gunPositionTransform;
        public Transform scopeInPositionTransform;
        public GameObject defaultGunModel;
        public GameObject reloadGunModel;

        private GunAmmoVisual gunAmmoVisual;
        
        [Header("Debug")]
        [SerializeField] private string overrideName = "RustyPistol";
        protected override void Awake() {
            base.Awake();
            playerActions = ClientInput.Singleton.GetPlayerActions();
            cam = Camera.main;
            gunAmmoVisual = GetComponentInChildren<GunAmmoVisual>();
        }

        protected override void OnEntityStart() {
            base.OnEntityStart();
            gunAmmoVisual.Init(BoundEntity);
        }

        protected override IEntity OnInitWeaponEntity(WeaponBuilder<RustyPistolEntity> builder) {
            return builder.OverrideName(overrideName).FromConfig().Build();
        }
        
        protected override void OnBindEntityProperty() {}
        
        public override void OnItemUse() {
            if (!isReloading) {
                if (BoundEntity.CurrentAmmo > 0 &&
                    Time.time > lastShootTime + BoundEntity.GetAttackSpeed().RealValue) {
                    lastShootTime = Time.time;

                    Shoot();

                    BoundEntity.CurrentAmmo.Value--;

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
                                StartCoroutine(ReloadChangeModel());
                            }
                        }
                    }
                }
            }
        }

        
        public override bool OnItemScopePressed(bool shouldScope) {
            if (isReloading) {
                return false;
            }
            if (!shouldScope) {
                StartCoroutine(ScopeOut());
            }
            else {
                StartCoroutine(ScopeIn());
            }
            return shouldScope;
        }


        public void Update()
        {
            if (isHolding && !playerModel.IsPlayerDead())
            {
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
                        StartCoroutine(ReloadChangeModel());
                    }
                }
                
            }
        }

        private IEnumerator ReloadChangeModel()
        {
            isReloading = true;

            defaultGunModel.SetActive(false);
            reloadGunModel.SetActive(true);

            yield return new WaitForSeconds(BoundEntity.GetReloadSpeed().BaseValue);
            
            defaultGunModel.SetActive(true);
            reloadGunModel.SetActive(false);
            isReloading = false;
            BoundEntity.Reload();
        }
        
        private IEnumerator ScopeIn()
        {
            float startTime = 0f;
            float amimationTime = 0.1f;

            while (startTime <= amimationTime)
            {
                model.transform.position = Vector3.Lerp(
                    gunPositionTransform.position,
                    scopeInPositionTransform.position,
                    startTime / amimationTime);
                
                startTime += Time.deltaTime;
                yield return null;
            }
            model.transform.position = scopeInPositionTransform.position;
            yield return null;
            isScopedIn = true;
            crossHairViewController?.OnScope(true);
        }

        private IEnumerator ScopeOut(bool reloadAfter = false)
        {
            float startTime = 0f;
            float amimationTime = 0.1f;

            while (startTime <= amimationTime)
            {
                model.transform.position = Vector3.Lerp(
                    scopeInPositionTransform.position,
                    gunPositionTransform.position,
                    startTime / amimationTime);

                startTime += Time.deltaTime;
                yield return null;
            }

            model.transform.position = gunPositionTransform.position;

            yield return null;
            isScopedIn = false;

            if (reloadAfter)
            {
                // isReloading = true;
                StartCoroutine(ReloadChangeModel());
            }
            crossHairViewController?.OnScope(false);
        }

        public override void OnRecycled() {
            base.OnRecycled();
            isScopedIn = false;
            isReloading = false;
            
            defaultGunModel.SetActive(true);
            reloadGunModel.SetActive(false);
        }
    }
}
