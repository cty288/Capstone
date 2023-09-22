using System;
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
		
		protected KeepGlobalRotation realHealthBarSpawnPoint = null;
		private HealthBar currentHealthBar = null;
		private bool isPointed = false;

		private Camera mainCamera = null;
		protected override void Awake() {
			base.Awake();
			mainCamera = Camera.main;
			if (healthBarSpawnPoint) {
				//make a copy of the spawn point
				realHealthBarSpawnPoint = Instantiate(healthBarSpawnPoint, healthBarSpawnPoint.parent)
					.GetComponent<KeepGlobalRotation>();

			}
		}

		protected override HealthBar OnSpawnHealthBar() {
			HealthBar bar = HUDManager.Singleton
				.SpawnHUDElement(realHealthBarSpawnPoint.transform, healthBarPrefabName, HUDCategory.HealthBar, true)
				.GetComponent<HealthBar>();
			
			//set its scale to health bar spawn point's scale
			bar.transform.localScale = realHealthBarSpawnPoint.transform.localScale;
			bar.gameObject.SetActive(false);
			currentHealthBar = bar;
			return bar;
		}

		public override void OnPointByCrosshair() {
			isPointed = true;
			base.OnPointByCrosshair();
			if (currentHealthBar) {
				currentHealthBar.gameObject.SetActive(true);
			}
		}

		public override void OnUnPointByCrosshair() {
			isPointed = false;
			base.OnUnPointByCrosshair();
			if (currentHealthBar) {
				currentHealthBar.gameObject.SetActive(false);
			}
		}

		private void FixedUpdate() {
			//if it is pointed, then make sure the health bar is not blocking the view
			//solution: raycast from the camera to realHealthBarSpawnPoint, if it hits the enemy, then move the health bar up until it doesn't hit the enemy
			//otherwise, move the health bar down to the original position
			//use raycast all
			if (isPointed && currentHealthBar) {
				RaycastHit[] hits = Physics.RaycastAll(mainCamera.transform.position,
					realHealthBarSpawnPoint.transform.position - mainCamera.transform.position,
					Vector3.Distance(mainCamera.transform.position, realHealthBarSpawnPoint.transform.position),
					LayerMask.GetMask("Enemy"));
				if (hits.Length > 0) {
					//hit the enemy, move the health bar up
					currentHealthBar.transform.position = hits[0].point;
				}
				else {
					//didn't hit the enemy, move the health bar down
					currentHealthBar.transform.position = realHealthBarSpawnPoint.transform.position;
				}
			}
		}

		protected override void OnDestroyHealthBar(HealthBar healthBar) {
			HUDManager.Singleton.DespawnHUDElement(healthBarSpawnPoint, HUDCategory.HealthBar);
			currentHealthBar = null;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			isPointed = false;
		}
	}
}