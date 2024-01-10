using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Systems;
using _02._Scripts.Runtime.Levels.ViewControllers;
using _02._Scripts.Runtime.Pillars.Commands;
using _02._Scripts.Runtime.Pillars.Models;
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
using UnityEngine.Serialization;

namespace _02._Scripts.Runtime.Pillars.Systems {
	public interface IPillarSystem : ISystem {
		
	}

	public struct OnBossSpawned {
		public IEnemyViewController Boss;
	}
	public class PillarActivateInfo {
		public CurrencyType pillarCurrencyType;
		public float currencyPercentage;
		public float currencyAmount;
		public int level;
		
		public PillarActivateInfo(CurrencyType pillarCurrencyType, float currencyAmount, float currencyPercentage, int level) {
			this.pillarCurrencyType = pillarCurrencyType;
			this.currencyPercentage = currencyPercentage;
			this.currencyAmount = currencyAmount;
			this.level = level;
		}
	}
	public struct OnPillarActivated {
		[FormerlySerializedAs("currencyPercentage")] public Dictionary<CurrencyType, PillarActivateInfo> Info;
		public bool isAllPillarsActivated;
	}
	public class PillarSystem : AbstractSystem, IPillarSystem {
		private Dictionary<CurrencyType, IPillarEntity> 
			currentLevelPillars = new Dictionary<CurrencyType, IPillarEntity>();

		private Dictionary<CurrencyType, PillarActivateInfo>
			currentLevelPillarCurrencyAmount = new Dictionary<CurrencyType, PillarActivateInfo>();
		
		private ILevelModel levelModel;
		private ILevelSystem levelSystem;

		protected override void OnInit() {
			this.RegisterEvent<OnSetCurrentLevelPillars>(OnSetCurrentLevelPillars);
			this.RegisterEvent<OnRequestActivatePillar>(OnRequestActivatePillar);
			levelModel = this.GetModel<ILevelModel>();
			levelSystem = this.GetSystem<ILevelSystem>();
		}

		private void OnRequestActivatePillar(OnRequestActivatePillar e) {
			if (currentLevelPillars.ContainsKey(e.pillarCurrencyType)) {
				IPillarEntity pillarEntity = currentLevelPillars[e.pillarCurrencyType];
				if (pillarEntity.Status.Value == PillarStatus.Idle) {
					ActivatePillar(pillarEntity, e.CurrencyAmount, e.level);
				}
			}
		}
		
		private void ActivatePillar(IPillarEntity pillarEntity, float currencyAmount, int level) {
			pillarEntity.Status.Value = PillarStatus.Activated;
			
			currentLevelPillarCurrencyAmount[pillarEntity.PillarCurrencyType] =
				new PillarActivateInfo(pillarEntity.PillarCurrencyType, currencyAmount, 0, level);
			
			CalculateCurrencyPercentage();
			
			
			

			//check if all pillars are activated
			bool allPillarsActivated = true;
			foreach (var pillar in currentLevelPillars) {
				if (pillar.Value.Status.Value != PillarStatus.Activated) {
					allPillarsActivated = false;
					break;
				}
			}
			this.SendEvent<OnPillarActivated>(new OnPillarActivated() {
				Info = currentLevelPillarCurrencyAmount,
				isAllPillarsActivated = allPillarsActivated
			});
			if (allPillarsActivated) {
				SummonBoss();
			}
		}

		//TODO: temporarily close the exit door
		private async UniTask SummonBoss() {
			string levelID = levelModel.CurrentLevel.Value.UUID;
			foreach (IPillarEntity pillarEntity in currentLevelPillars.Values) {
				pillarEntity.Status.Value = PillarStatus.Spawning;
			}
			
			
			await UniTask.WaitForSeconds(Random.Range(8f, 15f));
			//make sure the level is not changed
			if (levelModel.CurrentLevel.Value.UUID != levelID) {
				return;
			}
			
			foreach (IPillarEntity pillarEntity in currentLevelPillars.Values) {
				pillarEntity.Status.Value = PillarStatus.Idle;
			}
			
			ILevelEntity levelEntity = levelModel.CurrentLevel.Value;
			if (levelEntity.IsInBossFight) {
				return;
			}
			
			Dictionary<CurrencyType, float> currencyPercentage = new Dictionary<CurrencyType, float>();
			foreach (var pillarCurrencyAmount in currentLevelPillarCurrencyAmount) {
				currencyPercentage.Add(pillarCurrencyAmount.Key, pillarCurrencyAmount.Value.currencyPercentage);
			}

			List<LevelSpawnCard> cards = levelModel.CurrentLevel.Value.GetAllBosses((bossEntity) => {
				IBossEntity boss = bossEntity as IBossEntity;
				return boss != null && boss.IsBossSpawnConditionSatisfied(currencyPercentage);
			});
			
			if (cards.Count > 0) {
            	LevelSpawnCard card = cards[Random.Range(0, cards.Count)];
            	GameObject prefabToSpawn = card.Prefabs[Random.Range(0, card.Prefabs.Count)];

                GameObject player = PlayerController.GetClosestPlayer(Vector3.zero).gameObject;
                Vector2 randomDir = Random.insideUnitCircle * 10;
                Vector3 randomPos = player.transform.position + new Vector3(randomDir.x, 0, randomDir.y);
                NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 1000,
	                NavMeshHelper.GetSpawnableAreaMask());
                
                
            	NavMeshFindResult res = await
            		SpawningUtility.FindNavMeshSuitablePosition(player,
            			() => prefabToSpawn.GetComponent<ICreatureViewController>().SpawnSizeCollider, 
                        hit.position, 90,
            			NavMeshHelper.GetSpawnableAreaMask(), default, 5, 5, 200);
            
            	 
            	
            	
            	Vector3 spawnPos = res.TargetPosition;
            	
            	if (float.IsInfinity(spawnPos.magnitude)) {
	                spawnPos = player.transform.position;
                }
            	
            	GameObject spawnedEnemy = CreatureVCFactory.Singleton.SpawnCreatureVC(prefabToSpawn, spawnPos, Quaternion.identity, null,
	                GetRarity(),
            		levelEntity.GetCurrentLevelCount(), true, 1, 10);
            	IEnemyEntity enemyEntity = spawnedEnemy.GetComponent<IEnemyViewController>().EnemyEntity;

            	//levelEntity.IsInBossFight.Value = true;
            	levelSystem.SetBossFight(enemyEntity);
            	
            	Vector3 spawnScale = spawnedEnemy.transform.localScale;
            	spawnedEnemy.gameObject.transform.localScale = Vector3.zero;
            	spawnedEnemy.transform.DOScale(spawnScale, 1f).SetEase(Ease.OutBack);
                
                this.SendEvent<OnBossSpawned>(new OnBossSpawned() {
	                Boss = spawnedEnemy.GetComponent<IEnemyViewController>()
				});
                
                ClearPillarCurrency();
			}
            
		}
		
		private void CalculateCurrencyPercentage() {
			float totalCurrencyAmount = 0;
			foreach (var pillarCurrencyAmount in currentLevelPillarCurrencyAmount) {
				totalCurrencyAmount += pillarCurrencyAmount.Value.currencyAmount;
			}
			
			Dictionary<CurrencyType, PillarActivateInfo> currencyPercentage = new Dictionary<CurrencyType, PillarActivateInfo>();
			foreach (var pillarCurrencyAmount in currentLevelPillarCurrencyAmount) {
				pillarCurrencyAmount.Value.currencyPercentage =
					pillarCurrencyAmount.Value.currencyAmount / totalCurrencyAmount;
			}
			
		}

		private int GetRarity() {
			int totalRarity = 0;
			foreach (var pillarCurrencyAmount in currentLevelPillarCurrencyAmount) {
				totalRarity += pillarCurrencyAmount.Value.level;
			}

			return Mathf.RoundToInt((float) totalRarity / currentLevelPillarCurrencyAmount.Count);
		}

		private void OnSetCurrentLevelPillars(OnSetCurrentLevelPillars e) {
			//new level, reset
			currentLevelPillars.Clear();
			currentLevelPillarCurrencyAmount.Clear();
			foreach (IPillarEntity pillarEntity in e.pillars) {
				currentLevelPillars.Add(pillarEntity.PillarCurrencyType, pillarEntity);
				
				currentLevelPillarCurrencyAmount.Add(pillarEntity.PillarCurrencyType,
					new PillarActivateInfo(pillarEntity.PillarCurrencyType, 0, 0, 0));
			}
			
		}

		private void ClearPillarCurrency() {
			currentLevelPillarCurrencyAmount.Clear();
			foreach (IPillarEntity pillarEntity in currentLevelPillars.Values) {
				currentLevelPillarCurrencyAmount.Add(pillarEntity.PillarCurrencyType,
					new PillarActivateInfo(pillarEntity.PillarCurrencyType, 0, 0, 0));
			}
		}
	}
}