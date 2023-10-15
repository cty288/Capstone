using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;


namespace a {
	public class Boss1Bullet : AbstractBulletViewController {

        public float bulletSpeed;
        private void Update()
        {
            //this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
        }
        protected override void OnHitResponse(HitData data) {
			
		}

        protected override void OnHitObject(Collider other) {
	        
        }

        protected override void OnBulletReachesMaxRange() {
	        
        }

        protected override void OnBulletRecycled() {
			
		}
        public void SetData(float bulletSpeed)
        {
            this.bulletSpeed = bulletSpeed;
        }
        

    }

	
	

	
}