using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using Mikrocosmos;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Player;
using Runtime.Temporary;
using Runtime.UI.NameTags;
using Runtime.Utilities;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Enemies.ViewControllers.Base {
	public abstract class AbstractNormalEnemyViewController<T> : AbstractEnemyViewController<T>, INormalEnemyViewController 
		where T : class, IEnemyEntity, new()  {
		[SerializeField] private string healthBarPrefabName = "EnemyHealthBar";
		[SerializeField] private Transform healthBarSpawnPoint = null;
		[SerializeField] private bool healthBarUIPositionAutoAdjust = true;
		
		[Header("Enemy Recycle Logic")]
		[SerializeField]
		private float autoRecycleTimeAfterFarAwayFromPlayer = 10f;
		[SerializeField]
		protected float autoRecycleDistanceFromPlayer = 150f;
		[SerializeField]
		protected float autoRecycleCheckTimeInterval = 5f;
		private Coroutine entityRemovalTimerCoroutine;
		private bool spawned;
		private float invincibleTime = 2f;
		protected Dictionary<HurtBox, bool> initialHurtBoxActiveState = new Dictionary<HurtBox, bool>();

		[SerializeField] private float eliteScaleMultiplier = 1.5f;
		protected override void Awake() {
			base.Awake();
			HurtBox[] hurtBoxes = GetComponentsInChildren<HurtBox>(true);
			foreach (var hurtBox in hurtBoxes) {
				initialHurtBoxActiveState.Add(hurtBox, hurtBox.gameObject.activeSelf);
			}
		}

		protected override void OnStart() {
			base.OnStart();
			if (autoRecycleTimeAfterFarAwayFromPlayer >= 0 && autoRecycleDistanceFromPlayer >= 0) {
				entityRemovalTimerCoroutine = StartCoroutine(EntityRemovalTimer());
			}
			
			foreach (var hurtBox in initialHurtBoxActiveState.Keys) {
				hurtBox.gameObject.SetActive(false);
			}

			BoundEntity.IsElite.RegisterWithInitValue(OnEliteChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		private void OnEliteChanged(bool arg1, bool isElite) {
			if (isElite) {
				transform.localScale *= eliteScaleMultiplier;
			}
			else {
				transform.localScale = Vector3.one;
			}
		}

		protected Transform GetPlayer() {
			return PlayerController.GetClosestPlayer(transform.position).transform;
		}


		protected override void Update() {
			base.Update();
			if (!spawned) {
				invincibleTime -= Time.deltaTime;
				if(invincibleTime < 0) {
					foreach (var hurtBox in initialHurtBoxActiveState.Keys) {
						hurtBox.gameObject.SetActive(initialHurtBoxActiveState[hurtBox]);
					}
					spawned = true;
				}
			}
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
			invincibleTime = 2f;
			spawned = false;
		}


		protected override HealthBar OnSpawnHealthBar() {
			HealthBar bar =
				SpawnCrosshairResponseHUDElement(healthBarSpawnPoint, healthBarPrefabName, HUDCategory.HealthBar, healthBarUIPositionAutoAdjust)
					.Item1.GetComponent<HealthBar>();
			return bar;
		}

		protected override void OnDestroyHealthBar(HealthBar healthBar) {
			DespawnHUDElement(healthBarSpawnPoint, HUDCategory.HealthBar);
		}
		
		
		
	}
}