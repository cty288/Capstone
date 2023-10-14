using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;




namespace a
{
	public class WormLazer : AbstractBulletViewController
	{
		private float bulletDamagePerTick;
		private float tickInterval;
		private float timer = 0.2f;
        private void Start()
        {
			tickType = true;
        }
        private void Update()
		{
			timer -= Time.deltaTime;
		}

		protected override void OnBulletReachesMaxRange()
		{

		}


		public void SetData(float damage , float tickInterval)
		{
			this.bulletDamagePerTick = damage;
			this.tickInterval = tickInterval;
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
        protected override void OnTriggerEnter(Collider other)
        {
			//not used
        }
		/*
        protected override void OnTriggerStay(Collider other)
        {
			
			if(timer < 0)
            {
				
				timer = 0.2f;
				base.OnTriggerStay(other);
            }
		}
		*/
		


    }





}