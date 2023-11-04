using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;



namespace a
{
	public class WheelBulletAlt : AbstractBulletViewController
	{
		
		private float timer = 5f;
		
		private void Start()
		{
			
		}
		private void Update()
		{
			this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * 10;
			timer -= Time.deltaTime;
			
			if (timer < 0)
			{
				RecycleToCache();
			}
		}

		protected override void OnBulletReachesMaxRange()
		{

		}


		public void SetData()
		{
			
		}

		protected override void OnHitResponse(HitData data)
		{

		}

		protected override void OnHitObject(Collider other)
		{

		}

		protected override void OnBulletRecycled()
		{
			timer = 2f;
		}


	}





}