using System;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using Framework;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.Utilities;
using Runtime.Player;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.Systems {
	public class LevelSystemExitEventHandler : ICanRegisterEvent {
		private ILevelEntity levelEntity;
		
		public void Init() {
			this.RegisterEvent<OnPlayerKillEnemy>(OnPlayerKillEnemy);
		}

		

		public void SetLevelEntity(ILevelEntity levelEntity) {
			this.levelEntity = levelEntity;
		}



		public void OnOneSecondPassed() {
			if(TryGetLevelExitCondition(out LevelExplorationCondition levelExplorationCondition)) {
				levelExplorationCondition.CurrentValue += levelExplorationCondition.ExplorationValuePerSecond;
				CheckLevelExitCondition();
			}
		}
		
		private void OnPlayerKillEnemy(OnPlayerKillEnemy e) {
			if(TryGetLevelExitCondition(out LevelExplorationCondition levelExplorationCondition)) {
				levelExplorationCondition.CurrentValue += e.Enemy.GetMaxHealth() * (e.IsBoss
					? levelExplorationCondition.BossExplorationMultiplier
					: levelExplorationCondition.NormalExplorationMultiplier);
				
				CheckLevelExitCondition();
			}
			
			if (e.IsBoss) {
				if(TryGetLevelExitCondition(out KillBossCondition condition)) {
					condition.KilledNumber++;
					CheckLevelExitCondition();
				}
			}

		}
		
		private bool TryGetLevelExitCondition<T>(out T levelExitCondition) where T : LevelExitCondition {
			levelExitCondition = null;
			
			if (levelEntity == null) {
				return false;
			}

			if (levelEntity.LevelExitConditions == null) {
				return false;
			}
			
			if (levelEntity.LevelExitConditions.TryGetValue(typeof(T), out var levelExitConditions)) {
				levelExitCondition = levelExitConditions as T;
				return true;
			}

			return false;
		}
		
		private void CheckLevelExitCondition() {
			if (levelEntity == null) {
				return;
			}

			if (levelEntity.LevelExitConditions == null) {
				return;
			}

			bool allSatisfied = true;
			foreach (LevelExitCondition exitCondition in levelEntity.LevelExitConditions.Values) {
				if (!exitCondition.IsSatisfied()) {
					allSatisfied = false;
					break;
				}
				
			}

			if (allSatisfied) {
				Debug.Log("Level Exit Condition Satisfied");
			}
		}
		
		
		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}