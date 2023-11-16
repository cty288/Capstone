using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using UnityEngine;


namespace a
{
	public class CarrierBullet : AbstractBulletViewController
	{
		private float bulletSpeed;
		public GameObject particlePrefab;
		private GameObject particleInstance;
		private SafeGameObjectPool pool;
        
        protected override void Update()
		{
			base.Update();
			this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
		}
		private void Start()
		{
			pool = GameObjectPoolManager.Singleton.CreatePool(particlePrefab, 50, 100);
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

		}


	}





}