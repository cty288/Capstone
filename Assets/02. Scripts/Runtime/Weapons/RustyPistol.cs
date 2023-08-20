using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;
using MikroFramework.Pool;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.ViewControllers.Base;

namespace Runtime.Weapons.Model.Base
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
        public Camera cam;
        protected LineRenderer lr;
        public LayerMask layer;
        public WeaponType type;

        [Header("Stats")] [SerializeField] protected float shootCD;
        protected float currentCD;
        [SerializeField] public float range;
        [SerializeField] private int m_damage = 10;

        [Header("Gun General Settings")] [SerializeField]
        protected Transform launchPoint;

        [Header("Projectile Settings")] [SerializeField]
        protected ProjectileStats proj;

        public GameObject projectile;
        // private List<HitBox> projectile_hitboxes = new();

        [Header("HitScan Settings")] [SerializeField]
        private HitScan hitScan;

        [SerializeField] private GameObject hitParticlePrefab;

        public int Damage => m_damage;

        protected override void Start()
        {
            base.Start();
            cam = Camera.main;
            lr = GetComponent<LineRenderer>();

            hitScan = new HitScan(cam, range, layer, this);
        }

        public void Update()
        {
            currentCD += Time.deltaTime;
        }

        public void FixedUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                if (currentCD >= shootCD)
                {
                    currentCD = 0;
                    Shoot();
                }
            }
        }

        public void Shoot()
        {
            if (type == WeaponType.Hitscan)
            {
                if (!hitScan.CheckHit())
                {
                    Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    DrawLine(launchPoint.position, ray.GetPoint(range));
                }

                StartCoroutine(Hitscan());
            }
            else
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
                Vector3 destination = Vector3.zero;
                if (Physics.Raycast(ray, out hit))
                {
                    destination = hit.point;
                }
                else
                {
                    destination = ray.GetPoint(range);
                }

                GameObject p = Instantiate(projectile);
                p.transform.rotation = transform.rotation;
                p.transform.position = launchPoint.position;
                p.GetComponent<Rigidbody>().velocity = (destination - launchPoint.position).normalized * proj.speed;
            }
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

        protected override void OnEntityStart()
        {
            Debug.Log("RustyPistol Start");
        }

        protected override IWeaponEntity OnInitEnemyEntity(WeaponBuilder<RustyPistolEntity> builder)
        {
            return builder.SetAllBasics(10).Build();
        }
    }
}