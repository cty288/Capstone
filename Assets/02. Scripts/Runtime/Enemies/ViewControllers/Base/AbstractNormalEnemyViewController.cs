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
		
		private HealthBar currentHealthBar = null;
		protected override HealthBar OnSpawnHealthBar() {
			HealthBar bar = HUDManager.Singleton
				.SpawnHUDElement(healthBarSpawnPoint, healthBarPrefabName, HUDCategory.HealthBar, true)
				.GetComponent<HealthBar>();
			
			//set its scale to health bar spawn point's scale
			bar.transform.localScale = healthBarSpawnPoint.localScale;
			bar.gameObject.SetActive(false);
			currentHealthBar = bar;
			return bar;
		}

		public override void OnPointByCrosshair() {
			base.OnPointByCrosshair();
			if (currentHealthBar) {
				currentHealthBar.gameObject.SetActive(true);
			}
		}

		public override void OnUnPointByCrosshair() {
			base.OnUnPointByCrosshair();
			if (currentHealthBar) {
				currentHealthBar.gameObject.SetActive(false);
			}
		}

		protected override void OnDestroyHealthBar(HealthBar healthBar) {
			HUDManager.Singleton.DespawnHUDElement(healthBarSpawnPoint, HUDCategory.HealthBar);
			currentHealthBar = null;
		}
		
	}
}