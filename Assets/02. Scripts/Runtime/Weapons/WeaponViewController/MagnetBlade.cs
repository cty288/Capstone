using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using DG.Tweening;
using MikroFramework;
using MikroFramework.AudioKit;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Inventory.Model;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Runtime.Weapons
{
    public class MagnetBladeEntity : WeaponEntity<MagnetBladeEntity>
    {
        [field: ES3Serializable] public override string EntityName { get; set; } = "MagnetBlade";

        public override bool Collectable => true;
        [field: ES3Serializable] public override int Width { get; } = 2;
        
        public override void OnRecycle()
        {
        }

        protected override void InitWeaponPartsSlots() {
            foreach (var t in Enum.GetValues(typeof(WeaponPartType))) {
                WeaponPartType weaponPartType = (WeaponPartType) t;
                if(weaponParts.ContainsKey(weaponPartType)) continue;
                weaponParts.Add(weaponPartType, new HashSet<WeaponPartsSlot>());
                
            }
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


        public override string OnGroundVCPrefabName { get; } = "MagnetBlade";

    }

    public class MagnetBlade : AbstractWeaponViewController<MagnetBladeEntity>
    {
        public GameObject bladePrefab;
        
        private SafeGameObjectPool bladePool;
        private Stack<MagnetBladeBullet> blades = new Stack<MagnetBladeBullet>();
        private bool isReloadingBlade;

        private bool isMelee = false;
        
        protected override void Awake() {
            base.Awake();
            bladePool = GameObjectPoolManager.Singleton.CreatePool(bladePrefab, 20, 50);
        }

        public override void OnStartHold(GameObject ownerGameObject)
        {
            base.OnStartHold(ownerGameObject);
            isReloadingBlade = false;
            blades.Clear();
            
            int bladeCount = blades.Count;
            for(int i = 0; i < BoundEntity.CurrentAmmo - bladeCount; i++) {
                InitializeBlade();
            }
            
            CheckReloadBlade();
        }

        public override void OnStopHold()
        {
            base.OnStopHold();
            foreach (var blade in blades)
            {
                bladePool.Recycle(blade.gameObject);
            }
            
            blades.Clear();
        }

        private void InitializeBlade()
        {
            MagnetBladeBullet blade = bladePool.Allocate().GetComponent<MagnetBladeBullet>();
            
            // put under camera follower
            Transform parent = ownerGameObject.transform.GetChild(5);
            blade.transform.SetParent(parent);
            blade.transform.localPosition = GetCurrentBladeLocalPos();
            blade.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            // blade.gameObject.SetActive(true);
            blades.Push(blade);
        }
        
        private Vector3 GetCurrentBladeLocalPos()
        {
            int index = blades.Count;
            if (index == 0)
            {
                return new Vector3(0, 0.4f, 0);
            }
            return index % 2 == 0 ? new Vector3((index-1) * 0.4f, 0.4f, -0.4f) : new Vector3(index * -0.4f, 0.4f, -0.4f);
        }
        
        protected override IEntity OnInitWeaponEntity(WeaponBuilder<MagnetBladeEntity> builder) {
            return builder.FromConfig().Build();
        }
        
        public override void SetShoot(bool shouldShoot)
        {
            // base.SetShoot(shouldShoot);
            if (shouldShoot) {
                Shoot();
            }
        }

        public override void OnItemScopePressed()
        {
            // TODO: use melee attack (need animation)
            // check melee cooldown / avaliability
            // start melee animation
            // set melee cooldown
                // turn on collider using animation events
                // turn off collider using animation events
                // toggle melee attack finished
        }
        
        public override void OnItemUse() {}
        
        public override void OnItemStartUse()
        {
            // For semi-auto gun
            if (!isReloading) {
                if (blades.Count > 0 &&
                    Time.time > lastShootTime + BoundEntity.GetAttackSpeed().RealValue) {
                    lastShootTime = Time.time;
                    SetShoot(true);
                    ShootEffects();
                }
            }
        }

        protected override void WeaponUpdate() {}

        protected override void Shoot()
        {
            Vector3 shootDir = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction;
            MagnetBladeBullet blade = blades.Pop();
            
            blade.Init(CurrentFaction.Value,
                BoundEntity.GetRealDamageValue(),
                gameObject, this, BoundEntity.GetRange().BaseValue, true);
            
            blade.Launch(shootDir, BoundEntity.GetBulletSpeed().RealValue);
            BoundEntity.CurrentAmmo.Value--;
            CheckReloadBlade();
        }

        private void CheckReloadBlade()
        {
            if (blades.Count < BoundEntity.GetAmmoSize().RealValue
                && !isReloadingBlade)
            {
                StartCoroutine(ReloadBlade());
            }
        }
        
        private IEnumerator ReloadBlade()
        {
            isReloadingBlade = true;
            
            yield return new WaitForSeconds(BoundEntity.GetReloadSpeed().RealValue);
            InitializeBlade();
            BoundEntity.CurrentAmmo.Value++;
            
            isReloadingBlade = false;
            CheckReloadBlade();
        }
        
        public override bool CheckHit(HitData data)
        {
            return data.Hurtbox.Owner != gameObject;
        }
        
        public override void HitResponse(HitData data) {
        }
    }
}