using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.DayNight;
using _02._Scripts.Runtime.Levels.Events;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using _02._Scripts.Runtime.Levels.Sandstorm;
using Cysharp.Threading.Tasks;
using Runtime.DataFramework.Entities;
using Runtime.Spawning;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers {
	public abstract class NormalLevelViewController<T> : LevelViewController<T>, ILevelViewController
		where T : class, ILevelEntity, new() {
		[SerializeField] private bool hasRandomBossEncounter = true;
		[Header("Exit Conditions")] 
		[SerializeField] private float totalExplortionValueRequired = 2000; 
		[SerializeField] private float bossExplorationMultiplier = 2f;
		[SerializeField] private float normalEnemyExplorationMultiplier = 1f;
		[SerializeField] private float explorationValuePerSecond = 1f;
		//[SerializeField] private int killBossRequired = 1;
		[SerializeField] private float[] sandstormProbability = new[] {0, 0.33f, 1f};
		[SerializeField] private bool spawnWeaponPartsTrader = true;

		public override ILevelEntity OnBuildNewLevel(int levelNumber) {
			//SpawningUtility.SpawnExitDoor()
			ILevelEntity levelEntity = base.OnBuildNewLevel(levelNumber);
			//levelEntity?.AddLevelExitCondition(new KillBossCondition(killBossRequired));
			levelEntity?.AddLevelExitCondition(new LevelExplorationCondition(totalExplortionValueRequired,
				bossExplorationMultiplier,
				normalEnemyExplorationMultiplier, explorationValuePerSecond));
			
			return levelEntity;
		}

		protected override void OnNewDay(OnNewDayStart e) {
			base.OnNewDay(e);
			HandleSandstormEvent();
			HandleRandomBossEncounterEvent();
		}

		public override async UniTask Init() {
			 await base.Init();
			 if (spawnWeaponPartsTrader) {
				 GameObject npc = await SpawningUtility.SpawnWeaponPartsNPC(gameObject, "WeaponPartsUpgradeNPC", maxExtent.bounds);
				 npc.transform.SetParent(transform);
			 }
			
		}

		private void HandleRandomBossEncounterEvent() {
			if(!hasRandomBossEncounter) return;
			
			if (Random.Range(0f, 1f) <= levelModel.RandomBossEncounterEventChance) {
				//spawn a random boss today
				int spawnTime = Random.Range(60, 180);
				int rarity = BoundEntity.GetCurrentLevelCount();
				rarity = Random.Range(rarity - 1, rarity + 1);
				rarity = Mathf.Clamp(rarity, 1, 4);

				gameEventSystem.AddEvent(RandomBossEncounterEvent.Allocate(rarity), spawnTime);
				levelModel.RandomBossEncounterEventChance = 0;
			}else {
				levelModel.RandomBossEncounterEventChance += 0.3f;
			}
		}


		protected void HandleSandstormEvent() {
			if (BoundEntity.DayStayed -1 >= sandstormProbability.Length) {
				return;
			}
			float sandstormProb = sandstormProbability[BoundEntity.DayStayed - 1];
			if (Random.Range(0f, 1f) <= sandstormProb) {
				//spawn sandstorm
				int sandstormHappenTime = 23 * 60;
				gameEventSystem.AddEvent(new SandstormEvent(), sandstormHappenTime);

				int warningTime = sandstormHappenTime / 2;
				gameEventSystem.AddEvent(new SandstormWarningEvent(), warningTime);
			}
			
			// Add Night Events
			// Night occurs at 8pm (20h)
			int nightHappeningTime = (GameTimeModel.NightStartHour - GameTimeModel.NewDayStartHour) * 60;
			gameEventSystem.AddEvent(new NightEvent(), nightHappeningTime);
			
			// Trigger warning 1 in-game hour before night time.
			gameEventSystem.AddEvent(NightWarningEvent.Allocate(60), nightHappeningTime - 60);
			
			// New Day Event
			gameEventSystem.AddEvent(new NewDayEvent(), 0);
		}
	}
}