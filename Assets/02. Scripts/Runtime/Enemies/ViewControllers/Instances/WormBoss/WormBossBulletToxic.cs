using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Instances.WormBoss {
    public class WormBossBulletToxic : AbstractBulletViewController
    {
        private float bulletSpeed;
        public GameObject particlePrefab;
        private SafeGameObjectPool pool;
        private GameObject particleInstance;
        private Rigidbody rb;
        
        private void Start()
        {
            pool = GameObjectPoolManager.Singleton.CreatePool(particlePrefab, 50, 100);
            rb = gameObject.GetComponent<Rigidbody>();
        }

        protected override void OnBulletReachesMaxRange() {}


        public void SetData(float bulletSpeed)
        {
            this.bulletSpeed = bulletSpeed;
            rb.velocity = gameObject.transform.forward * bulletSpeed;
            print($"WORM BOSS BULLET SPEED {bulletSpeed}");
        }

        protected override void OnHitResponse(HitData data) {}

        protected override void OnHitObject(Collider other)
        {
            print($"WORM BOSS HIT {other.name}");
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
            pool.Recycle(particleInstance);   
        }
    }
}