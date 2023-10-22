using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using Mikrocosmos;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.Enemies.Model;
using Runtime.Player;
using Runtime.Temporary;
using Runtime.UI.NameTags;
using UnityEngine;

namespace Runtime.Enemies.ViewControllers.Base {
	public abstract class AbstractNormalEnemyViewController<T> : AbstractEnemyViewController<T> 
		where T : class, IEnemyEntity, new()  {
		[SerializeField] private string healthBarPrefabName = "EnemyHealthBar";
		[SerializeField] private Transform healthBarSpawnPoint = null;
		
		[Header("Enemy Recycle Logic")]
		[SerializeField]
		private float autoRecycleTimeAfterFarAwayFromPlayer = 10f;
		[SerializeField]
		protected float autoRecycleDistanceFromPlayer = 150f;
		[SerializeField]
		protected float autoRecycleCheckTimeInterval = 5f;
		private Coroutine entityRemovalTimerCoroutine;

		protected override void OnStart() {
			base.OnStart();
			if (autoRecycleTimeAfterFarAwayFromPlayer >= 0 && autoRecycleDistanceFromPlayer >= 0) {
				entityRemovalTimerCoroutine = StartCoroutine(EntityRemovalTimer());
			}
		}

		protected Transform GetPlayer() {
			return PlayerController.GetClosestPlayer(transform.position).transform;
		}
		
		private IEnumerator EntityRemovalTimer() {
			while (true) {
				
				if (Vector3.Distance(GetPlayer().position, transform.position) > autoRecycleDistanceFromPlayer) {
					yield return new WaitForSeconds(autoRecycleTimeAfterFarAwayFromPlayer);
					
					if (Vector3.Distance(GetPlayer().position, transform.position) > autoRecycleDistanceFromPlayer) {
						enemyModel.RemoveEntity(BoundEntity.UUID);
						yield break;
					}
				}
				
				yield return new WaitForSeconds(autoRecycleCheckTimeInterval);
			}
		}

		protected override void OnReadyToRecycle() {
			base.OnReadyToRecycle();
			if (entityRemovalTimerCoroutine != null) {
				StopCoroutine(entityRemovalTimerCoroutine);
				entityRemovalTimerCoroutine = null;
			}
		}


		protected override HealthBar OnSpawnHealthBar() {
			HealthBar bar =
				SpawnCrosshairResponseHUDElement(healthBarSpawnPoint, healthBarPrefabName, HUDCategory.HealthBar)
					.Item1.GetComponent<HealthBar>();
			return bar;
		}

		protected override void OnDestroyHealthBar(HealthBar healthBar) {
			DespawnHUDElement(healthBarSpawnPoint, HUDCategory.HealthBar);
		}
		
	}
}