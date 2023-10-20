using Polyglot;

namespace _02._Scripts.Runtime.Levels.Models.LevelPassCondition {
	public class KillBossCondition : LevelExitCondition {
		[field: ES3Serializable]
		public int RequiredNumber { get; protected set; }
		
		[field: ES3Serializable]
		public int KilledNumber { get; set; }
		
		public KillBossCondition(int requiredNumber) {
			this.RequiredNumber = requiredNumber;
			KilledNumber = 0;
		}
		
		public KillBossCondition() {
			
		}

		public override string GetDescription() {
			return Localization.GetFormat("WIN_CONDITION_KILL_BOSS", RequiredNumber);
		}

		public override bool IsSatisfied() {
			return KilledNumber >= RequiredNumber;
		}
	}
}