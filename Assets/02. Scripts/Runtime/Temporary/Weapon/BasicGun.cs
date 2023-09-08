using System;
using System.Collections;
using Mikrocosmos.Controls;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Temporary.Weapon
{
    public enum WeaponType
    {
        Hitscan,
        Projectile,
    }


    [System.Serializable]
    public struct ProjectileStats
    {
        public int damage;
        public float speed;
        public ProjectileStats(int dmg, float spd)
        {
            damage = dmg;
            speed = spd;
        }
    }

    public class BasicGun : MonoBehaviour, IHitResponder
    {
        public Camera cam;
        protected LineRenderer lr;
        public LayerMask layer;
        public WeaponType type;


        [Header("Stats")]
        [SerializeField] protected float shootCD;
        protected float currentCD;
        [SerializeField] public float range;
        [SerializeField] private int m_damage = 10;

        [Header("Gun General Settings")]
        [SerializeField] protected Transform launchPoint;

        [Header("Projectile Settings")]
        [SerializeField] protected ProjectileStats proj;
        public GameObject projectile;
        // private List<HitBox> projectile_hitboxes = new();

        [Header("HitScan Settings")]
        [SerializeField] private HitScan hitScan;
        [SerializeField] private GameObject hitParticlePrefab;

        public int Damage => m_damage;
        private DPunkInputs.PlayerActions playerActions;

        private void Awake() {
            playerActions = ClientInput.Singleton.GetPlayerActions();
        }

        public void Start()
        {
            cam = Camera.main;
            lr = GetComponent<LineRenderer>();

            // hitScan = new HitScan(cam, range, layer, this);
        }

        public void Update()
        {
            currentCD += Time.deltaTime;
        }

        public void FixedUpdate()
        {
            if (playerActions.Shoot.IsPressed())
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
                // if (!hitScan.CheckHit())
                // {
                //     Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                //     DrawLine(launchPoint.position, ray.GetPoint(range));
                // }
                StartCoroutine(Hitscan());
            }
            else
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
                Vector3 destination = Vector3.zero;
                if (Physics.Raycast(ray, out hit)) {
                    destination = hit.point;
                }
                else {
                    destination = ray.GetPoint(range);
                }
                
                //TODO: object pools
                GameObject p = Instantiate(projectile);
                p.transform.rotation = transform.rotation;
                p.transform.position = launchPoint.position;
                p.GetComponent<Bullet>().Init(Faction.Friendly, 10); //temp
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
            if (data.Hurtbox.Owner == gameObject) { return false; }
            else { return true; }
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