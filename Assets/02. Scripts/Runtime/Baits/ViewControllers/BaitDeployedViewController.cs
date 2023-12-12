using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Baits.Model.Base;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Systems;
using _02._Scripts.Runtime.Levels.ViewControllers;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MikroFramework.Architecture;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.AI;

public class BaitDeployedViewController : AbstractDeployableResourceViewController<BaitEntity> {
	private ILevelModel levelModel;
	protected ParticleSystem[] particleSystems;
	[SerializeField] protected Vector2 baitWaitTimeRange = new Vector2(5, 15);
	private ILevelSystem levelSystem;

	protected override void Awake() {
		base.Awake();
		levelModel = this.GetModel<ILevelModel>();
		levelSystem = this.GetSystem<ILevelSystem>();
		particleSystems = GetComponentsInChildren<ParticleSystem>(true);
	}

	protected override void OnEntityStart() {
		foreach (var system in particleSystems) {
			system.Stop();
		}
	}

	protected override void OnBindEntityProperty() {
		
	}

	public override void OnDeployed() {
		foreach (var system in particleSystems) {
			system.loop = true;
			system.Play();
		}

		BaitSpawnBoss();
	}

	protected async Task BaitSpawnBoss() {
		BoundEntity.BaitStatus = BaitStatus.Baiting;
		//yield return new WaitForSeconds(Random.Range(baitWaitTimeRange.x, baitWaitTimeRange.y));

		await UniTask.WaitForSeconds(Random.Range(baitWaitTimeRange.x, baitWaitTimeRange.y), false,
			PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycle() );
		
		foreach (var system in particleSystems) {
			system.loop = false;
		}
		//yield return new WaitForSeconds(4f);
		await UniTask.WaitForSeconds(4f,false,
			PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycle());
		
		BoundEntity.BaitStatus = BaitStatus.Deactivated;
		ILevelEntity levelEntity = levelModel.CurrentLevel.Value;
		if (levelEntity.IsInBossFight) {
			return;
		}
		
		List<LevelSpawnCard> cards = levelModel.CurrentLevel.Value.GetAllBosses((entity => {
			return entity.GetProperty<IVigilianceProperty>().RealValue.Value
				>= BoundEntity.GetVigiliance().Value && entity.GetProperty<ITasteProperty>().RealValue.Value.TrueForAll(
					taste => BoundEntity.GetTaste().Value.Contains(taste));
		}));

		if (cards.Count > 0) {
			LevelSpawnCard card = cards[Random.Range(0, cards.Count)];
			GameObject prefabToSpawn = card.Prefabs[Random.Range(0, card.Prefabs.Count)];
			NavMeshFindResult task = await SpawningUtility.FindNavMeshSuitablePosition(
					gameObject,
					() => prefabToSpawn.GetComponent<ICreatureViewController>().SpawnSizeCollider, transform.position, 90,
					NavMeshHelper.GetSpawnableAreaMask(), default, 5, 3, 20);
			

			Vector3 spawnPos = task.TargetPosition;
			
			
			if (!float.IsInfinity(spawnPos.magnitude)) {
				int baitRarity = BoundEntity.GetRarity();
				int spawnRarity = Random.Range(baitRarity - 1, baitRarity + 2);
				GameObject spawnedEnemy = CreatureVCFactory.Singleton.SpawnCreatureVC(prefabToSpawn, spawnPos, Quaternion.identity, null, spawnRarity,
					levelEntity.GetCurrentLevelCount(), true, 1, 10);
				IEnemyEntity enemyEntity = spawnedEnemy.GetComponent<IEnemyViewController>().EnemyEntity;

				//levelEntity.IsInBossFight.Value = true;
				levelSystem.SetBossFight(enemyEntity);
				
				Vector3 spawnScale = spawnedEnemy.transform.localScale;
				spawnedEnemy.gameObject.transform.localScale = Vector3.zero;
				spawnedEnemy.transform.DOScale(spawnScale, 1f).SetEase(Ease.OutBack);
				
				Debug.Log($"Spawn Success: {enemyEntity.EntityName} at {spawnPos} with rarity {spawnRarity}");
			}
		}

	}

	protected override IResourceEntity OnBuildNewEntity(bool isPreview) {
		return null;
	}

	public override bool CheckCanDeploy(Vector3 slopeNormal, Vector3 position, bool isAir, out DeployFailureReason failureReason,
		out Quaternion spawnedRotation) {
		if (levelModel.CurrentLevel.Value.IsInBossFight) {
			failureReason = DeployFailureReason.BaitInBattle;
			spawnedRotation = Quaternion.identity;
			return false;
		}
		
		//areas except NotWalkable (layer 1)
	
		//int notWalkableArea = NavMesh.GetAreaFromName("Not Walkable");
		//int layerMask = ~(1 << notWalkableArea);

		if (!NavMesh.SamplePosition(position, out _, 1f, NavMeshHelper.GetSpawnableAreaMask())) {
			failureReason = DeployFailureReason.Obstructed;
			spawnedRotation = Quaternion.identity;
			return false;
		}
		return base.CheckCanDeploy(slopeNormal, position, isAir, out failureReason, out spawnedRotation);
	}

	public override void OnRecycled() {
		base.OnRecycled();
		foreach (var system in particleSystems) {
			system.Stop();
		}
		transform.localScale = Vector3.one;
		DOTween.Kill(transform);
		DOTween.Kill(this);
	}
}
