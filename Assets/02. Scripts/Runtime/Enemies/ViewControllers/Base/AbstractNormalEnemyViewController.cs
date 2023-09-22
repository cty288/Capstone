using MikroFramework;
using MikroFramework.Pool;
using Runtime.Enemies.Model;
using Runtime.UI.NameTags;
using UnityEngine;

namespace Runtime.Enemies.ViewControllers.Base {
	public abstract class AbstractNormalEnemyViewController<T> : AbstractEnemyViewController<T> 
		where T : class, IEnemyEntity, new()  {
		[SerializeField] private string healthBarPrefabName = "EnemyHealthBar";
		[SerializeField] private Transform healthBarSpawnPoint = null;
		protected override HealthBar OnSpawnHealthBar() {
			HealthBar bar = HUDManager.Singleton
				.SpawnHUDElement(healthBarSpawnPoint, healthBarPrefabName, HUDCategory.HealthBar, true)
				.GetComponent<HealthBar>();
			
			//set its scale to health bar spawn point's scale
			bar.transform.localScale = healthBarSpawnPoint.localScale;
			
			return bar;
		}

		protected override void OnDestroyHealthBar(HealthBar healthBar) {
			HUDManager.Singleton.DespawnHUDElement(healthBarSpawnPoint, HUDCategory.HealthBar);
		}
	}
}