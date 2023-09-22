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

		protected LayerMask crossHairDetectLayerMask;
		protected override void Awake() {
			base.Awake();
			mainCamera = Camera.main;
			if (healthBarSpawnPoint) {
				//make a copy of the spawn point
				realHealthBarSpawnPoint = Instantiate(healthBarSpawnPoint, healthBarSpawnPoint.parent)
					.GetComponent<KeepGlobalRotation>();

			}
			crossHairDetectLayerMask = LayerMask.GetMask("CrossHairDetect");
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
			if (currentHealthBar) {
				Vector3 targetPos = currentHealthBar.transform.position;
			
				//if it is pointed, then make sure the health bar is not blocking the view
				//solution: raycast from the camera to realHealthBarSpawnPoint, if it hits the enemy, then move the health bar up until it doesn't hit the enemy
				//otherwise, move the health bar down to the original position
				if (isPointed) {
					var camTr = mainCamera.transform;
				
					RaycastHit hit;
					if (Physics.Raycast(camTr.position, realHealthBarSpawnPoint.transform.position - camTr.position, out hit, 100f, crossHairDetectLayerMask)) {
						targetPos += Vector3.up * 0.5f;
						//if the player is right below (or very close in terms of x and z) the enemy, we need to also move the health bar in both x and z axis until it doesn't hit the enemy
						if (Math.Abs(hit.point.x - realHealthBarSpawnPoint.transform.position.x) < 2f &&
						    Math.Abs(hit.point.z - realHealthBarSpawnPoint.transform.position.z) < 2f) {

							targetPos += new Vector3(0.5f, 0, 0.5f);
						}
					}
					else {
						targetPos = healthBarSpawnPoint.transform.position;
					}
				}

				currentHealthBar.transform.position =
					Vector3.Lerp(currentHealthBar.transform.position, targetPos, 0.1f);
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