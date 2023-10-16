using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCircleCollider2D;
using JetBrains.Annotations;
using MikroFramework;
using MikroFramework.Architecture;
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
        public GameObject defaultGunModel;
        public GameObject reloadGunModel;

        public Transform bulletSpawnPos;
        public GameObject bulletPrefab;
        
        private SafeGameObjectPool pool;
        
        protected override void OnEntityStart()
        {
            base.OnEntityStart();
            
            playerModel = this.GetModel<IGamePlayerModel>();
        }
        
        protected override void Awake() {
            base.Awake();
            playerActions = ClientInput.Singleton.GetPlayerActions();
            cam = Camera.main;
            
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab, 20, 50);
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
                    
                    Debug.Log("Shoot");
                    Shoot();

                    BoundEntity.CurrentAmmo.Value--;

                    
                }
                
                if (autoReload && BoundEntity.CurrentAmmo <= 0)
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

        public override void Shoot()
        {
            base.Shoot();
            Vector3 shootDir = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction;
            
            GameObject b = pool.Allocate();
            b.transform.position = bulletSpawnPos.position;
            b.transform.rotation = Quaternion.identity;
            
            b.GetComponent<Rigidbody>().velocity = shootDir * BoundEntity.GetBulletSpeed().RealValue;

            b.GetComponent<IBulletViewController>().Init(CurrentFaction.Value,
                BoundEntity.GetBaseDamage().RealValue,
                gameObject, gameObject.GetComponent<ICanDealDamage>(), BoundEntity.GetRange().BaseValue);
        }
        
        public override void OnItemScopePressed() {
            if (isReloading) {
                return;
            }
            if (isScopedIn) {
                StartCoroutine(ScopeOut());
            }
            else {
                StartCoroutine(ScopeIn());   
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
        }

        public override void OnRecycled() {
            base.OnRecycled();

        }

        protected override void OnReadyToRecycle() {
            base.OnReadyToRecycle();
            isScopedIn = false;
            isReloading = false;
            
            defaultGunModel.SetActive(true);
            reloadGunModel.SetActive(false);
        }
    }
}
