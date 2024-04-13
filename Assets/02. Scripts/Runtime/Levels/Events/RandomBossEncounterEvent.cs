using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Systems;
using _02._Scripts.Runtime.Levels.ViewControllers;
using _02._Scripts.Runtime.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MikroFramework.Architecture;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies;
using Runtime.Enemies.Model;
using Runtime.Spawning;
using Runtime.Temporary;
using UnityEngine;
using UnityEngine.AI;

namespace _02._Scripts.Runtime.Levels.Events {
	public struct RandomBossEncounterEventTriggered {
		public IEnemyEntity BossEntity;
	}
	public class RandomBossEncounterEvent : GameEvent<RandomBossEncounterEvent>, ICanGetModel {
		[field: ES3Serializable]
		public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
		
		
		[field: ES3Serializable]
		public override bool CanPersistToOtherLevels { get; } = false;
		
		[field: ES3Serializable]
		private int rarity;
		
		private IGameEventSystem gameEventSystem;
		private ILevelSystem levelSystem;
		private ILevelModel levelModel;
		public override void OnInitialized() {
			levelSystem = this.GetSystem<ILevelSystem>();
			gameEventSystem = this.GetSystem<IGameEventSystem>();
			levelModel = this.GetModel<ILevelModel>();

			Debug.Log($"A random boss encounter event will trigger in {RemainingMinutesToTrigger} minutes!");
		}

		public override void OnTriggered() {
			if (levelSystem.IsInBossFight()) {
				gameEventSystem.AddEvent(RandomBossEncounterEvent.Allocate(rarity), 30);
				return;
			}
			
			SpawnBoss().Forget();
		}


		private async UniTask SpawnBoss() {
			List<LevelSpawnCard> cards = levelModel.CurrentLevel.Value.GetAllBosses();
			if(cards == null || cards.Count == 0) {
				return;
			}
			
			LevelSpawnCard card = cards[Random.Range(0, cards.Count)];
			GameObject prefabToSpawn = card.Prefabs[Random.Range(0, card.Prefabs.Count)];


			var pillars =  GameObject.FindGameObjectsWithTag("Pillar");
			if (pillars.Length == 0) {
				return;
			}
			
			GameObject pillar = pillars[Random.Range(0, pillars.Length)];
			
			
			NavMeshFindResult res = await
				SpawningUtility.FindNavMeshSuitablePosition(null,
					() => prefabToSpawn.GetComponent<ICreatureViewController>().SpawnSizeCollider,
					pillar.transform.position, 90,
					NavMeshHelper.GetSpawnableAreaMask(), default, 5, 5, 200);
			
			Vector3 spawnPos = res.TargetPosition;
				
			if (float.IsInfinity(spawnPos.magnitude)) {
				spawnPos = pillar.transform.position;
			}
				
			GameObject spawnedEnemy = CreatureVCFactory.Singleton.SpawnCreatureVC
			(prefabToSpawn, spawnPos, Quaternion.identity, null, rarity,
				rarity, true, 1, 10);
			IEnemyEntity enemyEntity = spawnedEnemy.GetComponent<IEnemyViewController>().EnemyEntity;

			//levelEntity.IsInBossFight.Value = true;
			levelSystem.SetBossFight(enemyEntity);
				
			Vector3 spawnScale = spawnedEnemy.transform.localScale;
			spawnedEnemy.gameObject.transform.localScale = Vector3.zero;
			spawnedEnemy.transform.DOScale(spawnScale, 1f).SetEase(Ease.OutBack);

			//onSpawnEnemy?.Invoke(spawnedEnemy, this);
			Debug.Log($"Spawn Success: {enemyEntity.EntityName} at {spawnPos} with rarity {rarity}");
			
			this.SendEvent<RandomBossEncounterEventTriggered>(new RandomBossEncounterEventTriggered() {
				BossEntity = enemyEntity
			});
		}

		public override void OnLeaped() {
			
		}

		
		
		
		public override void OnEventRecycled() {
			
		}
		
		public static RandomBossEncounterEvent Allocate(int rarity) {
			RandomBossEncounterEvent e = RandomBossEncounterEvent.Allocate();
			e.rarity = rarity;
			return e;
			
		}
	}
}