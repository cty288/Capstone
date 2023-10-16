using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;


namespace a {
	public class Boss1Bullet : AbstractBulletViewController {

        public float bulletSpeed;
        private Transform playerTrans;
        private float timer;

        private void Start()
        {

            timer = Random.Range(0.5f, 2f);
        }
        private void Update()
        {
            timer -= Time.deltaTime;
            if(timer > 0)
            {
                this.gameObject.transform.LookAt(playerTrans);
            }
            else
            {
                if (playerTrans != null)
                {
                    // Calculate the direction from the bullet to the player.
                    Vector3 directionToPlayer = playerTrans.position - transform.position;

                    // Calculate the rotation needed to face the player.
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                    // Interpolate the bullet's rotation towards the target rotation using Slerp.
                    float rotationSpeed = 0.5f; // Adjust the speed of the rotation.
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                    // Move the bullet forward in its new direction.
                    transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
                }
            }
            
        }
        protected override void OnHitResponse(HitData data) {
			
		}

        protected override void OnHitObject(Collider other) {
	        
        }

        protected override void OnBulletReachesMaxRange() {
	        
        }

        protected override void OnBulletRecycled() {
            timer = Random.Range(0.5f, 2f);
        }
        public void SetData(float bulletSpeed, Transform playerTrans)
        {
            this.bulletSpeed = bulletSpeed;
            this.playerTrans = playerTrans;
        }
        

    }

	
	

	
}