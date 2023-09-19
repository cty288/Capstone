using MikroFramework;
using MikroFramework.Pool;
using Runtime.Enemies.Model;
using UnityEngine;

namespace Runtime.Enemies.ViewControllers.Base {
	public abstract class AbstractBossViewController<T> : AbstractEnemyViewController<T> where T : class, IEnemyEntity, new()  {
		
		[SerializeField] private string healthBarPrefabName = "UIHealthBar";
		protected override HealthBar OnSpawnHealthBar() {
			GameObjectPool pool =
				GameObjectPoolManager.Singleton.CreatePoolFromAB(healthBarPrefabName, null, 1, 10,
					out GameObject prefab);

			HealthBar bar = pool.Allocate().GetComponent<HealthBar>();
			UIHealthBarZone.Singleton.SpawnHealthBarObject(bar);
			return bar;
		}

		protected override void OnDestroyHealthBar(HealthBar healthBar) {
			GameObjectPoolManager.Singleton.Recycle(healthBar.gameObject);
		}
	}
}