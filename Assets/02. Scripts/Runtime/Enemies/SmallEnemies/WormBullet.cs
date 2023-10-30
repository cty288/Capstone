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
        [SerializeField] private float maxRotationAngle = 60f;
        private float timer = 0.5f;

        private void Update()
        {
            timer -= Time.deltaTime;
            if (player != null && timer > 0)
            {
                Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                Vector3 directionToPlayer = (player.transform.position + offset - transform.position).normalized;

             
                float angle = Vector3.Angle(transform.forward, directionToPlayer);

               
                if (angle <= maxRotationAngle)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            
                    // Adjust the rotation speed by multiplying with rotationSpeed
                    float t = rotationSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
                }

                transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
            }
            else
            {
                this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
            }
            
        }

        public void SetData(float bulletSpeed, GameObject player , float rotationSpeed)
        {
            this.bulletSpeed = Random.Range(bulletSpeed - 3, bulletSpeed + 3);
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
            timer = 0.5f;
        }

        protected override void OnBulletReachesMaxRange()
        {
            
        }
    }
}
