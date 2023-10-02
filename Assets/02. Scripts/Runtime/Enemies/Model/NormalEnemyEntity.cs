using Runtime.Utilities.ConfigSheet;

namespace Runtime.Enemies.Model {

	public interface INormalEnemyEntity : IEnemyEntity {
		
	}
	public abstract class NormalEnemyEntity<T> : EnemyEntity<T>, INormalEnemyEntity where T : NormalEnemyEntity<T>, new()  {
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.NormalEnemyEntityConfigTable;
		}
	}
}