using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Instances.WormBoss {
    public class WormBulletToxic : AbstractBulletViewController
    {
        private float bulletSpeed;
        public GameObject particlePrefab;
        private SafeGameObjectPool pool;
        private GameObject particleInstance;
        private void Start()
        {
            pool = GameObjectPoolManager.Singleton.CreatePool(particlePrefab, 50, 100);
        }

        protected override void Update()
        {
            base.Update();
            gameObject.GetComponent<Rigidbody>().velocity = gameObject.transform.forward * bulletSpeed;
            overrideExplosionFaction = true;
        }

        protected override void OnBulletReachesMaxRange() {}


        public void SetData(float bulletSpeed)
        {
            this.bulletSpeed = bulletSpeed;
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
                particleInstance.transform.position = (hitPoint);
                particleInstance.transform.rotation = Quaternion.LookRotation(hitNormal);
                
             
            }
        }

        protected override void OnBulletRecycled()
        {
            // pool.Recycle(particleInstance);   
        }
    }
}