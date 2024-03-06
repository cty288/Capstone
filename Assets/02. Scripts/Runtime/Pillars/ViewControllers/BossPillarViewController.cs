using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Baits.Model.Base;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.Properties;
using _02._Scripts.Runtime.Levels.Systems;
using _02._Scripts.Runtime.Levels.ViewControllers;
using _02._Scripts.Runtime.Pillars.Models;
using _02._Scripts.Runtime.Rewards;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.Controls;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Properties;
using Runtime.Spawning.Commands;
using Runtime.UI;
using Runtime.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;
using Random = UnityEngine.Random;

//using PropertyName = UnityEngine.PropertyName;

namespace Runtime.Spawning.ViewControllers.Instances {
	public interface IBossPillarViewController {
		//IEntity OnBuildNewEntity(int level, Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts);
		//void SetBossSpawnCosts(Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts);

		string InitPillar(ILevelEntity levelEntity,
			Dictionary<CurrencyType, RewardCostInfo> rewardCosts, PillarRewardsInfo rewardsInfo);
		BoxCollider SpawnSizeCollider { get; }
		
		IPillarEntity Entity { get; }
	}
	
	
	public class BossPillarViewController : AbstractBasicEntityViewController<PillarEntity>, IBossPillarViewController {
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;
		protected IPillarModel pillarModel;
		protected ILevelEntity LevelEntity;
		protected int levelNumber;
		
		
		protected ILevelModel levelModel;
		protected ParticleSystem[] particleSystems;
		//protected bool isActivating = false;
		protected ILevelSystem levelSystem;
		[SerializeField] private Vector2 waitTimeRange = new Vector2(10, 20);
		[SerializeField] private GameObject normalTrail;
		[SerializeField] private GameObject activatedTrail;
		[SerializeField] private Transform rewardSpawnPos;
		protected override void Awake() {
			base.Awake();
			pillarModel = this.GetModel<IPillarModel>();
			levelModel = this.GetModel<ILevelModel>();
			levelSystem = this.GetSystem<ILevelSystem>();
			particleSystems = GetComponentsInChildren<ParticleSystem>(true);
		}

		protected override IEntity OnBuildNewEntity() {
			return OnBuildNewEntity(1, new Dictionary<CurrencyType, RewardCostInfo>(), default);
		}
		
		public IEntity OnBuildNewEntity(int level,
			Dictionary<CurrencyType, RewardCostInfo> rewardCosts, PillarRewardsInfo rewardsInfo) {
			
			PillarBuilder<PillarEntity> builder = pillarModel.GetPillarBuilder<PillarEntity>();
			builder.SetProperty(new PropertyNameInfo(PropertyName.level_number), level).SetRewardCost(rewardCosts)
				.SetRewardsInfo(rewardsInfo);
			return builder.Build();
		}

		protected override void OnEntityStart() {
			
			levelModel.CurrentLevel.Value.IsInBossFight.RegisterOnValueChanged(OnBossFightStatusChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			this.RegisterEvent<OnRequestPillarSpawnBoss>(OnRequestPillarSpawnBoss)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			BoundEntity.Status.RegisterWithInitValue(OnPillarStatusChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
			foreach (var system in particleSystems) {
				system.Stop();
			}
			
			normalTrail.gameObject.SetActive(true);
		}

		private void OnPillarStatusChanged(PillarStatus oldStatus, PillarStatus newStatus) {
			UpdateInteractHint();
			normalTrail.gameObject.SetActive(newStatus == PillarStatus.Idle);
			activatedTrail.gameObject.SetActive(newStatus == PillarStatus.Activated);

			if (newStatus == PillarStatus.Spawning) {
				PlaySpawningParticles();
			}
			else {
				StopSpawningParticles();
			}
		}


		private void OnBossFightStatusChanged(bool arg1, bool status) {
			UpdateInteractHint();
			normalTrail.gameObject.SetActive(!status);
		}

		protected override (InputAction, string, string) GetInteractHintInfo() {
			var b = base.GetInteractHintInfo();
			if (levelModel.CurrentLevel.Value.IsInBossFight.Value) {
				return (null, Localization.Get("PILLAR_ERROR_COMBAT"), b.Item3);
			}

			switch (BoundEntity.Status.Value) {
				case PillarStatus.Idle:
					return b;
				case PillarStatus.Spawning:
					return (null, Localization.Get("DEPLOY_ERROR_ACTIVATING"), b.Item3);
				case PillarStatus.Activated:
					return (null, Localization.Get("DEPLOY_ERROR_ACTIVATED"), b.Item3);
			}

			return b;
			
		}
		
		protected void UpdateInteractHint() {
			if (currentInteractiveHint != null) {
				(InputAction, string, string) hintInfo = GetInteractHintInfo();
				currentInteractiveHint.SetHint(hintInfo.Item1, hintInfo.Item2, hintInfo.Item3);
			}
		}

		protected override void OnBindEntityProperty() {
			
		}
		
		
		public string InitPillar(ILevelEntity levelEntity,
			Dictionary<CurrencyType, RewardCostInfo> rewardCosts, PillarRewardsInfo rewardsInfo) {
		
			LevelEntity = levelEntity;
			levelNumber = levelEntity.GetCurrentLevelCount();
			IEntity ent = OnBuildNewEntity(levelNumber, rewardCosts, rewardsInfo);
			InitWithID(ent.UUID);
            
			//for some reason we need to do this again
			LevelEntity = levelEntity;
			levelNumber = levelEntity.GetCurrentLevelCount();
			return ent.UUID;
		}



		private void PlaySpawningParticles() {
			foreach (var system in particleSystems) {
				system.loop = true;
				system.Play();
			}
		}
		
		private void StopSpawningParticles() {
			foreach (var system in particleSystems) {
				system.loop = false;
			}
		}

		[field: SerializeField]
		public BoxCollider SpawnSizeCollider { get; protected set; }

		public IPillarEntity Entity => BoundEntity;

		protected async UniTask SpawnBoss(int rarity) {
			Debug.Log("Spawn boss");
			PlaySpawningParticles();
			normalTrail.gameObject.SetActive(false);
			BoundEntity.Status.Value = PillarStatus.Spawning;
			UpdateInteractHint();

			await UniTask.WaitForSeconds(Random.Range(waitTimeRange.x, waitTimeRange.y),false,
				PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
			
			StopSpawningParticles();

			await UniTask.WaitForSeconds(4f,false,
				PlayerLoopTiming.Update,gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
		
			BoundEntity.Status.Value = PillarStatus.Idle;
			
			ILevelEntity levelEntity = levelModel.CurrentLevel.Value;
			if (levelEntity.IsInBossFight) {
				return;
			}

			List<LevelSpawnCard> cards = levelModel.CurrentLevel.Value.GetAllBosses();

			if (cards.Count > 0) {
				LevelSpawnCard card = cards[Random.Range(0, cards.Count)];
				GameObject prefabToSpawn = card.Prefabs[Random.Range(0, card.Prefabs.Count)];
				
				NavMeshFindResult res = await
					SpawningUtility.FindNavMeshSuitablePosition(gameObject,
						() => prefabToSpawn.GetComponent<ICreatureViewController>().SpawnSizeCollider, transform.position, 90,
						NavMeshHelper.GetSpawnableAreaMask(), default, 5, 5, 200);
			
				 
				
				
				Vector3 spawnPos = res.TargetPosition;
				
				if (float.IsInfinity(spawnPos.magnitude)) {
					spawnPos = transform.position;
				}
				
				GameObject spawnedEnemy = CreatureVCFactory.Singleton.SpawnCreatureVC(prefabToSpawn, spawnPos, Quaternion.identity, null, rarity,
					levelEntity.GetCurrentLevelCount(), true, 1, 10);
				IEnemyEntity enemyEntity = spawnedEnemy.GetComponent<IEnemyViewController>().EnemyEntity;

				//levelEntity.IsInBossFight.Value = true;
				levelSystem.SetBossFight(enemyEntity);
				
				Vector3 spawnScale = spawnedEnemy.transform.localScale;
				spawnedEnemy.gameObject.transform.localScale = Vector3.zero;
				spawnedEnemy.transform.DOScale(spawnScale, 1f).SetEase(Ease.OutBack);

				//onSpawnEnemy?.Invoke(spawnedEnemy, this);
				Debug.Log($"Spawn Success: {enemyEntity.EntityName} at {spawnPos} with rarity {rarity}");
			}
			else {
				UpdateInteractHint();
			}
		}
		
		
		
		
		private void OnRequestPillarSpawnBoss(OnRequestPillarSpawnBoss e) {
			if (e.Pillar == gameObject) {
				SpawnBoss(e.Rarity);
			}
		}

		protected override void OnPlayerPressInteract() {
			base.OnPlayerPressInteract();
			AudioSystem.Singleton.Play2DSound("activate_pillar");
			if(levelModel.CurrentLevel.Value.IsInBossFight.Value 
			   || UIManager.Singleton.GetPanel<PillarUIViewController>(true) != null
			   || BoundEntity.Status.Value != PillarStatus.Idle) {
				return;
			}

			this.SendCommand(OpenPillarUICommand.Allocate(BoundEntity,
				BoundEntity.RewardCost, rewardSpawnPos));
		}

		public override void OnRecycled() {
			base.OnRecycled();
		}
	}
}