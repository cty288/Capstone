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
	
	public struct OnPillarCurrencyReset {
		
	}
	
	[ES3Serializable]
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
		[FormerlySerializedAs("currencyPercentage")]
		public Dictionary<string, PillarActivateInfo> Info;
		public bool isAllPillarsActivated;
	}
	public class PillarSystem : AbstractSystem, IPillarSystem {

		private List<IPillarEntity> allPillars = new List<IPillarEntity>();

		
		private IPillarModel model;
		private bool isSpawning = false;

		[field: ES3Serializable]
		private int summonCount = 4;
		
		private ILevelModel levelModel;
		private ILevelSystem levelSystem;

		protected override void OnInit() {
			this.RegisterEvent<OnSetCurrentLevelPillars>(OnSetCurrentLevelPillars);
			this.RegisterEvent<OnRequestActivatePillar>(OnRequestActivatePillar);
			levelModel = this.GetModel<ILevelModel>();
			levelSystem = this.GetSystem<ILevelSystem>();
			model = this.GetModel<IPillarModel>();
		}

		private void OnRequestActivatePillar(OnRequestActivatePillar e) {
			if (!model.ActivatedPillarCurrencyAmount.ContainsKey(e.pillarEntity.UUID)) {
				if (e.pillarEntity.Status.Value == PillarStatus.Idle) {
					ActivatePillar(e.pillarEntity, e.pillarCurrencyType, e.CurrencyAmount, e.level);
				}
			}
		}
		
		private void ActivatePillar(IPillarEntity pillarEntity, CurrencyType currencyType,  float currencyAmount, int level) {
			pillarEntity.Status.Value = PillarStatus.Activated;

			model.ActivatedPillarCurrencyAmount.Add(pillarEntity.UUID,
				new PillarActivateInfo(currencyType, currencyAmount, 0, level));
			
			//[pillarEntity] =new PillarActivateInfo(currencyType, currencyAmount, 0, level);
			
			CalculateCurrencyPercentage();
			
			
			

			//check if all pillars are activated
			bool allPillarsActivated = model.ActivatedPillarCurrencyAmount.Count == allPillars.Count;
			
			this.SendEvent<OnPillarActivated>(new OnPillarActivated() {
				Info = model.ActivatedPillarCurrencyAmount,
				isAllPillarsActivated = allPillarsActivated
			});
			if (allPillarsActivated) {
				SummonBoss();
			}
		}

		//TODO: temporarily close the exit door
		private async UniTask SummonBoss() {
			if(allPillars == null || allPillars.Count == 0) {
				return;
			}
			string levelID = levelModel.CurrentLevel.Value.UUID;
			foreach (IPillarEntity pillarEntity in allPillars) {
				pillarEntity.Status.Value = PillarStatus.Spawning;
			}
			
			
			isSpawning = true;
			await UniTask.WaitForSeconds(Random.Range(8f, 15f));
			isSpawning = false;
			//make sure the level is not changed
			if (levelModel.CurrentLevel.Value.UUID != levelID) {
				return;
			}
			
			foreach (IPillarEntity pillarEntity in allPillars) {
				pillarEntity.Status.Value = PillarStatus.Idle;
			}
			
			ILevelEntity levelEntity = levelModel.CurrentLevel.Value;
			if (levelEntity.IsInBossFight) {
				ClearPillarCurrency();
				return;
			}
			
			Dictionary<CurrencyType, float> currencyPercentage = new Dictionary<CurrencyType, float>();
			foreach (var pillarCurrencyAmount in model.ActivatedPillarCurrencyAmount) {
				currencyPercentage.TryAdd(pillarCurrencyAmount.Value.pillarCurrencyType, 0);
				currencyPercentage[pillarCurrencyAmount.Value.pillarCurrencyType] +=
					pillarCurrencyAmount.Value.currencyPercentage;
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
                
               
			}
			ClearPillarCurrency();
		}
		
		private void CalculateCurrencyPercentage() {
			float totalCurrencyAmount = 0;
			foreach (var pillarCurrencyAmount in model.ActivatedPillarCurrencyAmount) {
				totalCurrencyAmount += pillarCurrencyAmount.Value.currencyAmount;
			}
			
			foreach (var pillarCurrencyAmount in model.ActivatedPillarCurrencyAmount) {
				pillarCurrencyAmount.Value.currencyPercentage =
					pillarCurrencyAmount.Value.currencyAmount / totalCurrencyAmount;
			}
			
		}

		private int GetRarity() {
			int totalRarity = 0;
			foreach (var pillarCurrencyAmount in model.ActivatedPillarCurrencyAmount) {
				totalRarity += pillarCurrencyAmount.Value.level;
			}

			return Mathf.RoundToInt((float) totalRarity / model.ActivatedPillarCurrencyAmount.Count);
		}

		private void OnSetCurrentLevelPillars(OnSetCurrentLevelPillars e) {
			//new level, reset
			this.allPillars.Clear();
			allPillars = new List<IPillarEntity>(e.pillars);
			//ClearPillarCurrency();
			if (e.levelCount == 0 || isSpawning) {
				isSpawning = false;
				ClearPillarCurrency();
			}
			else {
				foreach (KeyValuePair<string,PillarActivateInfo> activateInfo in model.ActivatedPillarCurrencyAmount) {
					activateInfo.Value.level = Mathf.Max(activateInfo.Value.level, e.levelCount);
				}
				this.SendEvent<OnPillarActivated>(new OnPillarActivated() {
					Info = model.ActivatedPillarCurrencyAmount,
					isAllPillarsActivated = false
				});
			}
		}

		private void ClearPillarCurrency() {
			model.ActivatedPillarCurrencyAmount.Clear();
			this.SendEvent<OnPillarCurrencyReset>(new OnPillarCurrencyReset());
		}
	}
}