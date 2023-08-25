using System.Collections;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCircleCollider2D;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Temporary.Player;
using Runtime.Temporary.Weapon;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Weapons
{
    public class RustyPistolEntity : WeaponEntity<RustyPistolEntity>
    {
        [field: SerializeField] public override string EntityName { get; protected set; } = "RustyPistol";
        
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
    }

    public class RustyPistol : AbstractWeaponViewController<RustyPistolEntity>, IHitResponder
    {
        private Camera cam;
        protected LineRenderer lr;
        public LayerMask layer;
        
        [Header("Timers & Counters")]
        private float currentCD;
        private int currentAmmo;
        private float currentReloadCD;
        
        [Header("Gun General Settings")] [SerializeField]
        protected Transform launchPoint;
        
        [Header("HitDetector Settings")] [SerializeField]
        private HitScan hitScan;
        private HitDetectorInfo hitDetectorInfo;
        
        [SerializeField] private GameObject hitParticlePrefab;
        
        // For IHitResponder.
        public int Damage => BoundEntity.GetBaseDamage().BaseValue;
        
        protected override void Start()
        {
            base.Start();
            cam = Camera.main;
            lr = GetComponent<LineRenderer>();
            
            hitScan = new HitScan(this);
            hitDetectorInfo = new HitDetectorInfo
            {
                camera = cam,
                layer = layer,
                lineRenderer = lr,
                launchPoint = launchPoint,
                weapon = BoundEntity
            };
        }
        
        protected override IEntity OnInitWeaponEntity(WeaponBuilder<RustyPistolEntity> builder) {
            return builder.FromConfig().Build();
        }
        
        protected override void OnBindEntityProperty() {}

        protected override void OnEntityStart()
        {
            currentAmmo = BoundEntity.GetAmmoSize().BaseValue;
        }
        
        public void Update()
        {
            currentCD += Time.deltaTime;
            
            if (currentReloadCD < BoundEntity.GetReloadSpeed().BaseValue)
            {
                currentReloadCD += Time.deltaTime;
            }
            else
            {
                currentAmmo = BoundEntity.GetAmmoSize().BaseValue;
            }
        }
        
        public void FixedUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                if (currentAmmo > 0 && currentCD >= BoundEntity.GetAttackSpeed().BaseValue)
                {
                    Shoot();
                    currentCD = 0;
                    currentAmmo--;
                    currentReloadCD = 0;
                }
            }
        }
        
        public void Shoot()
        {
            hitScan.CheckHit(hitDetectorInfo);
        }
        
        public bool CheckHit(HitData data)
        {
            if (data.Hurtbox.Owner == gameObject)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        public void HitResponse(HitData data)
        {
            Instantiate(hitParticlePrefab, data.HitPoint, Quaternion.identity);
            
            //TODO: Change to non-temporary class of player entity.
            PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
            playerMovement.rb.AddForce(data.Recoil * -data.HitDirectionNormalized, ForceMode.Impulse);
        }
    }
}