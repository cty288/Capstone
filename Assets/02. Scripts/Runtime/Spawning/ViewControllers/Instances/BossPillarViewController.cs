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
using _02._Scripts.Runtime.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MikroFramework.Architecture;
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
	public interface IBossPillarViewController : IDirectorViewController {
		//IEntity OnBuildNewEntity(int level, Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts);
		void SetBossSpawnCosts(Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts);
		
		BoxCollider SpawnSizeCollider { get; }
	}
	public class BossPillarViewController : AbstractBasicEntityViewController<BossPillarEntity>, IDirectorViewController, IBossPillarViewController {
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;
		protected IDirectorModel directorModel;
		protected ILevelEntity LevelEntity;
		protected int levelNumber;
		protected Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts;
		protected Action<GameObject, IDirectorViewController> onSpawnEnemy;
		protected ILevelModel levelModel;
		protected ParticleSystem[] particleSystems;
		protected bool isActivating = false;
		protected ILevelSystem levelSystem;
		[SerializeField] private Vector2 waitTimeRange = new Vector2(10, 20);
		
		protected override void Awake() {
			base.Awake();
			directorModel = this.GetModel<IDirectorModel>();
			levelModel = this.GetModel<ILevelModel>();
			levelSystem = this.GetSystem<ILevelSystem>();
			particleSystems = GetComponentsInChildren<ParticleSystem>(true);
		}

		protected override IEntity OnBuildNewEntity() {
			return OnBuildNewEntity(1, new Dictionary<CurrencyType, LevelBossSpawnCostInfo>());
		}
		
		public IEntity OnBuildNewEntity(int level, Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts) {
			DirectorBuilder<BossPillarEntity> builder = directorModel.GetDirectorBuilder<BossPillarEntity>();
			builder.SetProperty(new PropertyNameInfo(PropertyName.level_number), level)
				//TODO: set other property base values here
				.SetProperty(new PropertyNameInfo(PropertyName.spawn_boss_cost), bossSpawnCosts);

			return builder.Build();
		}

		protected override void OnEntityStart() {
			levelModel.CurrentLevel.Value.IsInBossFight.RegisterOnValueChanged(OnBossFightStatusChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			this.RegisterEvent<OnRequestPillarSpawnBoss>(OnRequestPillarSpawnBoss)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
			foreach (var system in particleSystems) {
				system.Stop();
			}
		}



		private void OnBossFightStatusChanged(bool arg1, bool status) {
			UpdateInteractHint();
		}

		protected override (InputAction, string, string) GetInteractHintInfo() {
			var b = base.GetInteractHintInfo();
			if (levelModel.CurrentLevel.Value.IsInBossFight.Value) {
				return (null, Localization.Get("PILLAR_ERROR_COMBAT"), b.Item3);
			}

			if (isActivating) {
				return (null, Localization.Get("DEPLOY_ERROR_ACTIVATING"), b.Item3);
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

		public void SetLevelEntity(ILevelEntity levelEntity) {
			LevelEntity = levelEntity;
			levelNumber = levelEntity.GetCurrentLevelCount();
			IEntity ent = OnBuildNewEntity(levelNumber, bossSpawnCosts);
			InitWithID(ent.UUID);
            
			//for some reason we need to do this again
			LevelEntity = levelEntity;
			levelNumber = levelEntity.GetCurrentLevelCount();
			bossSpawnCosts = BoundEntity.GetProperty<ISpawnBossCost>().RealValue.Value;
		}

		public IUnRegister RegisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy) {
			this.onSpawnEnemy += onSpawnEnemy;
			return new DirectorOnSpawnEnemyUnregister(this, onSpawnEnemy);
		}

		public void UnregisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy) {
			this.onSpawnEnemy -= onSpawnEnemy;
		}

		public void SetBossSpawnCosts(Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts) {
			this.bossSpawnCosts = bossSpawnCosts;
		}

		[field: SerializeField]
		public BoxCollider SpawnSizeCollider { get; protected set; }

		protected async UniTask SpawnBoss(int rarity) {
			Debug.Log("Spawn boss");
			foreach (var system in particleSystems) {
				system.loop = true;
				system.Play();
			}
			
			isActivating = true;
			UpdateInteractHint();

			await UniTask.WaitForSeconds(Random.Range(waitTimeRange.x, waitTimeRange.y));
			
			foreach (var system in particleSystems) {
				system.loop = false;
			}

			await UniTask.WaitForSeconds(4f);
		
			isActivating = false;
			
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
						NavMeshHelper.GetSpawnableAreaMask(), null, 5, 5, 200);
			
				 
				
				
				Vector3 spawnPos = res.TargetPosition;
				
				if (!float.IsInfinity(spawnPos.magnitude)) {
					GameObject spawnedEnemy = CreatureVCFactory.Singleton.SpawnCreatureVC(prefabToSpawn, spawnPos, Quaternion.identity, null, rarity,
						levelEntity.GetCurrentLevelCount(), true, 1, 10);
					IEnemyEntity enemyEntity = spawnedEnemy.GetComponent<IEnemyViewController>().EnemyEntity;

					//levelEntity.IsInBossFight.Value = true;
					levelSystem.SetBossFight(enemyEntity);
				
					Vector3 spawnScale = spawnedEnemy.transform.localScale;
					spawnedEnemy.gameObject.transform.localScale = Vector3.zero;
					spawnedEnemy.transform.DOScale(spawnScale, 1f).SetEase(Ease.OutBack);

					onSpawnEnemy?.Invoke(spawnedEnemy, this);
					Debug.Log($"Spawn Success: {enemyEntity.EntityName} at {spawnPos} with rarity {rarity}");
				}
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
			
			if(levelModel.CurrentLevel.Value.IsInBossFight.Value 
			   || UIManager.Singleton.GetPanel<PillarUIViewController>(true) != null
			   || isActivating) {
				return;
			}

			this.SendCommand(OpenPillarUICommand.Allocate(gameObject,
				BoundEntity.GetProperty<ISpawnBossCost>().RealValue.Value));
		}

		public override void OnRecycled() {
			base.OnRecycled();
			isActivating = false;
		}
	}
}