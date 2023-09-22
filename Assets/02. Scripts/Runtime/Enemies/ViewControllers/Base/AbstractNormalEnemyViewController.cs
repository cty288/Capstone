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
		
		protected KeepGlobalRotation realHealthBarSpawnPoint = null;
		private HealthBar currentHealthBar = null;
		private bool isPointed = false;

		private Camera mainCamera = null;

		protected LayerMask healthBarDetectLayerMask;
		private Vector3 healthBarTargetPos = Vector3.zero;
		private Vector3 originalHealthBarPos = Vector3.zero;
		protected override void Awake() {
			base.Awake();
			mainCamera = Camera.main;
			if (healthBarSpawnPoint) {
				//make a copy of the spawn point
				realHealthBarSpawnPoint = Instantiate(healthBarSpawnPoint, healthBarSpawnPoint.parent)
					.GetComponent<KeepGlobalRotation>();
				originalHealthBarPos = realHealthBarSpawnPoint.PositionOffset;

			}
			healthBarDetectLayerMask = LayerMask.GetMask("CrossHairDetect");
			//get all layers except for the crosshair detect layer
			
			healthBarDetectLayerMask = ~healthBarDetectLayerMask;
			
		}

		protected override void OnStart() {
			base.OnStart();
			if (currentHealthBar) {
				healthBarTargetPos = originalHealthBarPos;
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
			if (currentHealthBar) {
				
			
				//if it is pointed, then make sure the health bar is not blocking the view
				//solution: raycast from the camera to realHealthBarSpawnPoint, if it hits the enemy, then move the health bar up until it doesn't hit the enemy
				//otherwise, move the health bar down to the original position
				if (isPointed) {
					var camTr = mainCamera.transform;
				
					RaycastHit hit;
					bool hitSelf = false;
					
					if (Physics.Raycast(camTr.position, healthBarSpawnPoint.transform.position - camTr.position, out hit, 100f, healthBarDetectLayerMask)) {
						if (hit.collider.attachedRigidbody && hit.collider.attachedRigidbody.gameObject == gameObject) {
							hitSelf = true;
							
							if (Physics.Raycast(camTr.position, realHealthBarSpawnPoint.transform.position - camTr.position, out hit, 100f, healthBarDetectLayerMask)) {
								if (hit.collider.attachedRigidbody && hit.collider.attachedRigidbody.gameObject == gameObject) {
									healthBarTargetPos += Vector3.up * 0.5f;
									//if the player is right below (or very close in terms of x and z) the enemy, we need to also move the health bar in both x and z axis until it doesn't hit the enemy
									if (Math.Abs(camTr.position.x - realHealthBarSpawnPoint.transform.position.x) < 10f &&
									    Math.Abs(camTr.position.z - realHealthBarSpawnPoint.transform.position.z) < 10f) {
										healthBarTargetPos += new Vector3(0.5f, 0, 0.5f);
									}
									
								}
							}
						}
					}
					
					
					
					if (!hitSelf) {
						healthBarTargetPos = originalHealthBarPos;
					}
				}

				/*realHealthBarSpawnPoint.transform.position =
					Vector3.Lerp(realHealthBarSpawnPoint.transform.position, healthBarTargetPos, 0.1f);*/
				/*DOTween.To(() => realHealthBarSpawnPoint.PositionOffset,
					x => realHealthBarSpawnPoint.PositionOffset = x, healthBarTargetPos, 0.1f);*/
				
				realHealthBarSpawnPoint.PositionOffset = Vector3.Lerp(realHealthBarSpawnPoint.PositionOffset, healthBarTargetPos, 0.1f);
				
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