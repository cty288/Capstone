using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers {
	public class GunBullet : AbstractBulletViewController {
		public GameObject explosionPrefab;
		private int explosionDamage;
		public float explosionSize;

		public override void Init(Faction faction, int damage ,GameObject bulletOwner, ICanDealDamage owner, float maxRange,
			bool ownerTriggerHitResponse = false, bool overrideExplosionFaction = false)
		{
			base.Init(faction, 0, bulletOwner, owner, maxRange, ownerTriggerHitResponse);
			explosionDamage = damage;
		}

		protected override void OnHitResponse(HitData data) {
			//Explode();
			//RecycleToCache();
		}

		protected override void OnHitObject(Collider other) {
			Explode();
		}

		protected override void OnBulletReachesMaxRange() {
			Explode();
		}

		protected override void OnBulletRecycled() {
			
		}
		
		private void Explode() {
			SafeGameObjectPool pool = GameObjectPoolManager.Singleton.CreatePool(explosionPrefab, 10, 100);

			GameObject exp = pool.Allocate();
            
			//Instantiate(explosion,transform.position,Quaternion.identity);
			exp.transform.position = transform.position;
			exp.transform.rotation = Quaternion.identity;
			exp.GetComponent<IExplosionViewController>().Init(overrideExplosionFaction
					? CurrentFaction.Value
					: Faction.Explosion, explosionDamage, explosionSize, this.gameObject,
				this);
		}
	}
}