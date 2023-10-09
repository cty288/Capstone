using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace a
{
    public class WormBullet : AbstractBulletViewController
    {
        private float bulletSpeed;
        private GameObject player; // Reference to the player's transform
        private float rotationSpeed;
        private float life = 2f;


        private void Update()
        {
            if (player != null)
            {
                Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                // Adjust the rotation speed by multiplying with rotationSpeed
                float t = rotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);

                transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
            }
            life -= Time.deltaTime;
            if(life < 0)
            {
                //how do i put this back to the pool?
                Destroy(this.gameObject);
            }
        }

        public void SetData(float bulletSpeed, GameObject player , float rotationSpeed)
        {
            this.bulletSpeed = bulletSpeed;
            this.player = player;
            this.rotationSpeed = rotationSpeed;
        }

        protected override void OnHitResponse(HitData data)
        {
            
        }

        protected override void OnHitObject(Collider other)
        {
            
        }

        protected override void OnBulletRecycled()
        {
            
        }

        protected override void OnBulletReachesMaxRange()
        {
            
        }
    }
}
