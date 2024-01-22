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
        public Rigidbody rigidbody;
        protected override void Update() {
            if (!inited) {
                return;
            }
			         
            if (isShooting && maxRange > 0 && Vector3.Distance(transform.position, origin) > maxRange)
            {
                OnBulletReachesMaxRange();
                RecycleToCache();
            }
        }
        
        public void Launch(Vector3 direction, float speed)
        {
            transform.SetParent(null);
            transform.rotation = Quaternion.LookRotation(direction);
            rigidbody.isKinematic = false;
            rigidbody.velocity = direction * speed;
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
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
        }
    }
}