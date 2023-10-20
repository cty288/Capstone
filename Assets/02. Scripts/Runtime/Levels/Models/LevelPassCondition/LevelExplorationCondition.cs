using Polyglot;

namespace _02._Scripts.Runtime.Levels.Models.LevelPassCondition {
	public class LevelExplorationCondition : LevelExitCondition {
		
		[field: ES3Serializable]
		public float TotalValue { get; protected set; }
		[field: ES3Serializable]
		public float CurrentValue { get; protected set; }
		
		[field: ES3Serializable]
		public float BossExplorationMultiplier { get; set; }
		[field: ES3Serializable]
		public float NormalExplorationMultiplier { get; set; }
		
		[field: ES3Serializable]
		public float ExplorationValuePerSecond { get; set; }
		
		public LevelExplorationCondition(float totalValue, float bossExplorationMultiplier, float normalExplorationMultiplier, float explorationValuePerSecond) {
			this.TotalValue = totalValue;
			CurrentValue = 0;
			this.BossExplorationMultiplier = bossExplorationMultiplier;
			this.NormalExplorationMultiplier = normalExplorationMultiplier;
			this.ExplorationValuePerSecond = explorationValuePerSecond;
		}

		public override string GetDescription() {
			return Localization.Get("WIN_CONDITION_EXPLORATION");
		}

		public override bool IsSatisfied() {
			return TotalValue >= CurrentValue;
		}
	}
}