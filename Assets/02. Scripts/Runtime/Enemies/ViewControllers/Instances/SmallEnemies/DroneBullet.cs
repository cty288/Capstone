using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;



namespace a
{
	public class DroneBullet : AbstractBulletViewController
	{
		public float bulletSpeed;
        private void Update()
        {
			this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
		}
        protected override void OnHitResponse(HitData data)
		{

		}

		protected override void OnBulletRecycled()
		{

		}


	}





}