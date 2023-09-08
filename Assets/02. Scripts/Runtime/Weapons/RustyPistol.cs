using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCircleCollider2D;
using JetBrains.Annotations;
using Mikrocosmos.Controls;
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
    }

    public class RustyPistol : AbstractWeaponViewController<RustyPistolEntity>, IHitResponder
    {
        private Camera cam;
        public GameObject lineRendererPrefab;
        private List<LineRenderer> lineRenderers;
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
        private DPunkInputs.PlayerActions playerActions;

        protected override void Awake() {
            base.Awake();
            playerActions = ClientInput.Singleton.GetPlayerActions();
        }

        protected override void Start()
        {
            base.Start();
            cam = Camera.main;
            
            lineRenderers = new List<LineRenderer>();
            for (int i = 0; i < BoundEntity.GetBulletsPerShot().BaseValue; i++)
            {
                Debug.Log("adding line renderer");
                lineRenderers.Add(Instantiate(lineRendererPrefab, transform).GetComponent<LineRenderer>());
            }
            
            hitScan = new HitScan(this);
            hitDetectorInfo = new HitDetectorInfo
            {
                camera = cam,
                layer = layer,
                lineRenderers = lineRenderers,
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
            if (playerActions.Shoot.IsPressed())
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