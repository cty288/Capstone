using Polyglot;

namespace _02._Scripts.Runtime.Levels.Models.LevelPassCondition {
	public class EnterExitCondition : LevelExitCondition{
		public override string GetDescription() {
			return Localization.Get("WIN_CONDITION_EXIT");
		}

		public override bool IsSatisfied() {
			return false;
		}
	}
}