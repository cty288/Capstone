using MikroFramework.ResKit;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Weapons
{
    public class MagnetBladeBullet : AbstractBulletViewController
    {
        private bool isShooting = false;
        
        protected override void Update() {
            if (!inited) {
                return;
            }
			         
            if (isShooting && maxRange > 0 && Vector3.Distance(transform.position, origin) > maxRange) {
                OnBulletReachesMaxRange();
                RecycleToCache();
            }
        }
        
        public void Launch(Vector3 direction, float speed)
        {
            print("LAUNCH BLADE");
            transform.SetParent(null);
            transform.rotation = Quaternion.identity;
            GetComponent<Rigidbody>().velocity = direction * speed;
            isShooting = true;
            origin = transform.position;
        }
        
        protected override void OnHitResponse(HitData data)
        {
        }

        protected override void OnHitObject(Collider other)
        {
        }

        protected override void OnBulletReachesMaxRange()
        {
        }

        protected override void OnBulletRecycled()
        {
            isShooting = false;
        }
    }
}