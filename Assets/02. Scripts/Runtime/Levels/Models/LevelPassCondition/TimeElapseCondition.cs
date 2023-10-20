using Polyglot;

namespace _02._Scripts.Runtime.Levels.Models.LevelPassCondition {
	public class TimeElapseCondition : LevelExitCondition {
		
		[field: ES3Serializable]
		public int TotalTime { get; protected set; }
		[field: ES3Serializable]
		public int ElapsedTime { get; protected set; }
		


		public TimeElapseCondition(int totalTime) {
			this.TotalTime = totalTime;
			ElapsedTime = 0;
		}

		public override string GetDescription() {
			return Localization.Get("WIN_CONDITION_EXPLORATION");
		}

		public override bool IsSatisfied() {
			return ElapsedTime >= TotalTime;
		}
	}
}