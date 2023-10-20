using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using Runtime.DataFramework.Entities;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers {
	public abstract class NormalLevelViewController<T> : LevelViewController<T>, ILevelViewController
		where T : class, ILevelEntity, new() {

		[Header("Exit Conditions")] 
		[SerializeField] private float totalExplortionValueRequired = 2000; 
		[SerializeField] private float bossExplorationMultiplier = 2f;
		[SerializeField] private float normalEnemyExplorationMultiplier = 1f;
		[SerializeField] private float explorationValuePerSecond = 1f;
		[SerializeField] private int killBossRequired = 1;
		
		protected override IEntity OnBuildNewEntity() {
			ILevelEntity levelEntity = base.OnBuildNewEntity() as ILevelEntity;

			levelEntity.LevelExitConditions = new List<LevelExitCondition>() {
				new KillBossCondition(killBossRequired),
				new LevelExplorationCondition(totalExplortionValueRequired, bossExplorationMultiplier,
					normalEnemyExplorationMultiplier, explorationValuePerSecond)
			};
			
			return levelEntity;
		}
	}
}