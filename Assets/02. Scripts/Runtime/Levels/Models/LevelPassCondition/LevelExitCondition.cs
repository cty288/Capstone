namespace _02._Scripts.Runtime.Levels.Models.LevelPassCondition {
	public abstract class LevelExitCondition {
		public abstract string GetDescription();
		public abstract bool IsSatisfied();
	}
}