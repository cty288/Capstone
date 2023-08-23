using System.Collections;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Temporary.Weapon;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        [Header("Timers")]
        private float currentCD;
        
        [Header("Gun General Settings")] [SerializeField]
        protected Transform launchPoint;
        
        [Header("HitScan Settings")] [SerializeField]
        private HitScan hitScan;
        
        [SerializeField] private GameObject hitParticlePrefab;
        
        // For IHitResponder.
        public int Damage => BoundEntity.GetBaseDamage().BaseValue;
        
        protected override void Start()
        {
            base.Start();
            cam = Camera.main;
            lr = GetComponent<LineRenderer>();
            
            hitScan = new HitScan(cam, BoundEntity.GetRange().BaseValue, layer, this);
        }
        
        protected override IEntity OnInitWeaponEntity(WeaponBuilder<RustyPistolEntity> builder) {
            return builder.FromConfig().Build();
        }
        
        protected override void OnBindEntityProperty() {}
        protected override void OnEntityStart() {}
        
        public void Update()
        {
            currentCD += Time.deltaTime;
        }
        
        public void FixedUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                if (currentCD >= BoundEntity.GetAttackSpeed().BaseValue)
                {
                    currentCD = 0;
                    Shoot();
                }
            }
        }
        
        public void Shoot()
        {
            if (!hitScan.CheckHit())
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                DrawLine(launchPoint.position, ray.GetPoint(BoundEntity.GetRange().BaseValue));
            }
    
            StartCoroutine(Hitscan());
        }
        
        IEnumerator Hitscan()
        {
            lr.enabled = true;
            yield return new WaitForSeconds(0.3f);
            lr.enabled = false;
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
            DrawLine(launchPoint.position, data.HitPoint);
        }
        
        public void DrawLine(Vector3 start, Vector3 end)
        {
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }
}