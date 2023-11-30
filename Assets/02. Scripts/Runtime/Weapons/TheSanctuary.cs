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
        public Transform bulletSpawnPos;
        public GameObject bulletPrefab;
        
        private SafeGameObjectPool pool;
        
        protected override void Awake() {
            base.Awake();
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab, 20, 50);
        }
        
        protected override IEntity OnInitWeaponEntity(WeaponBuilder<TheSanctuaryEntity> builder) {
            return builder.FromConfig().Build();
        }
        
        public override void SetShoot(bool shouldShoot)
        {
            base.SetShoot(shouldShoot);
            if (shouldShoot) {
                Shoot();
            }
        }

        protected override void Shoot()
        {
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
}
