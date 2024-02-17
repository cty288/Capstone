using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Instances.WormBoss {
    public class WormBossBulletToxic : AbstractBulletViewController
    {
        public GameObject particlePrefab;
        private SafeGameObjectPool pool;
        private GameObject particleInstance;

        private int acidTickDamage;
        private float acidExplosionRadius;
        
        [SerializeField] private Rigidbody rb;
        
        private void Start()
        {
            pool = GameObjectPoolManager.Singleton.CreatePool(particlePrefab, 3, 9);
        }

        protected override void OnBulletReachesMaxRange()
        {
            particleInstance = pool.Allocate();
            particleInstance.transform.position = transform.position;
        }


        public void SetData(float bulletSpeed, int acidTickDamage, float acidExplosionRadius)
        {
            this.acidTickDamage = acidTickDamage;
            this.acidExplosionRadius = acidExplosionRadius;
            rb.velocity = gameObject.transform.forward * bulletSpeed;
        }

        protected override void OnHitResponse(HitData data) {}

        protected override void OnHitObject(Collider other)
        {
            if (particlePrefab != null)
            {
                // Get the hit point and normal
                Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
                Vector3 hitNormal = other.ClosestPointOnBounds(transform.position + transform.forward) - transform.position;

                // Instantiate the particle system at the hit point with the correct rotation
                particleInstance = pool.Allocate();
                particleInstance.transform.position = hitPoint;
                particleInstance.transform.rotation = Quaternion.LookRotation(hitNormal);
                
                particleInstance.GetComponent<WormBossAcidExplosionViewController>().Init(Faction.Hostile, acidTickDamage, acidExplosionRadius, gameObject, owner);
            }
        }

        protected override void OnBulletRecycled()
        {
            // pool.Recycle(particleInstance);   
        }
    }
}