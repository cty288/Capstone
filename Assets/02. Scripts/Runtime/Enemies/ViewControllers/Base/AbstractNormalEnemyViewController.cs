using System;
using DG.Tweening;
using Mikrocosmos;
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
		
		

		protected override void OnStart() {
			base.OnStart();
		}

		protected override HealthBar OnSpawnHealthBar() {
			HealthBar bar =
				SpawnCrosshairResponseHUDElement(healthBarSpawnPoint, healthBarPrefabName, HUDCategory.HealthBar)
					.GetComponent<HealthBar>();
			return bar;
		}

		protected override void OnDestroyHealthBar(HealthBar healthBar) {
			DespawnHUDElement(healthBarSpawnPoint, HUDCategory.HealthBar);
		}
		
	}
}