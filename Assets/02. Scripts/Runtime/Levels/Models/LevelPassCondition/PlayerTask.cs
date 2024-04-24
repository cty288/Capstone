namespace _02._Scripts.Runtime.Levels.Models.LevelPassCondition {
	public abstract class PlayerTask {
		public abstract string GetDescription();
		public abstract bool IsSatisfied();

		public abstract void OnFinish();
	}
}