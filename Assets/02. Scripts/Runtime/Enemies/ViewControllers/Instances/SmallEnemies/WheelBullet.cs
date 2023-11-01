using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;
using Cinemachine;
using Runtime.Enemies.SmallEnemies;
using UnityEngine.AI;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Unity;




namespace a
{
	public class WheelBullet : AbstractBulletViewController
	{
		private float bulletSpeed;
		private float timer = 2f;
		public SharedGameObject bullet;
		private Transform followGameObject;
		private SafeGameObjectPool pool;
		private Transform target;
		public float numBullets;
		public float fanAngle;
		private Faction f;
		private int dmg;
		private GameObject bulletOwner;
		private ICanDealDamage owner;
		private int what;

		
		private void Start()
        {
			pool = GameObjectPoolManager.Singleton.CreatePool(bullet.Value, 30, 50);
			
		}
        private void Update()
		{
			
			timer -= Time.deltaTime;
			this.transform.position = followGameObject.transform.up * 0.828f + followGameObject.right * 0.013f + followGameObject.transform.forward * 0.114f + followGameObject.position;
			if (timer < 0)
			{
				for (int i = 0; i < 5; i++)
				{
					
					GameObject bulletInstance = pool.Allocate();
					bulletInstance.GetComponent<IBulletViewController>().Init(f, dmg, bulletOwner, owner, what);
					

					if (bulletInstance != null)
					{

						// Calculate the angle for this bullet within the fan shape
						float angle = (i / (float)(numBullets - 1) - 0.5f) * fanAngle;
						Quaternion rotation = Quaternion.Euler(0, angle, 0);  // Create a rotation based on the angle

						// Set the position and direction of the allocated bullet
						bulletInstance.transform.position = transform.position;
						bulletInstance.transform.forward = rotation * (target.position - transform.position).normalized;
					}
				}

				// Recycle the main bullet to the object pool
				RecycleToCache();
			}
		}

		protected override void OnBulletReachesMaxRange()
		{

		}


		public void SetData(float bulletSpeed, Transform target , Transform player, Faction value, int v, GameObject gameObject, ICanDealDamage canDealDamage, int v1 ,float numBullet, float angle)
		{
			this.bulletSpeed = bulletSpeed;
			this.followGameObject = target;
			this.target = player;
			f = value;
			dmg = v;
			bulletOwner = gameObject;
			owner = canDealDamage;
			what = v1;
			this.numBullets = numBullet;
			this.fanAngle = angle;

			
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