using Polyglot;

namespace _02._Scripts.Runtime.Levels.Models.LevelPassCondition {
	public class KillBossCondition : LevelExitCondition {
		[field: ES3Serializable]
		public int RequiredNumber { get; protected set; }
		
		[field: ES3Serializable]
		public int KilledNumber { get; protected set; }
		
		public KillBossCondition(int requiredNumber) {
			this.RequiredNumber = requiredNumber;
			KilledNumber = 0;
		}

		public override string GetDescription() {
			return Localization.Get("WIN_CONDITION_KILL_BOSS");
		}

		public override bool IsSatisfied() {
			return KilledNumber >= RequiredNumber;
		}
	}
}