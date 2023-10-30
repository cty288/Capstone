using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;



namespace a
{
	public class WheelBullet : AbstractBulletViewController
	{
		private float bulletSpeed;
		private float timer = 2f;
		public GameObject bullet;
        private void Start()
        {
			
        }
        private void Update()
		{
			//this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
			timer -= Time.deltaTime;
			if(timer < 0)
            {
				
			}
		}

		protected override void OnBulletReachesMaxRange()
		{

		}


		public void SetData(float bulletSpeed)
		{
			this.bulletSpeed = bulletSpeed;
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


	}





}