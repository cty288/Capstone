using _02._Scripts.Runtime.Common.Entities.Enemies;

namespace _02._Scripts.Runtime.Base.Entity {
	public interface IEntityBuilderFactory {
		EntityBuilder<T> GetBuilder<T>() where T : class, IEntity, new();
	}
	
	public class EntityBuilderFactory : IEntityBuilderFactory {

		public EntityBuilder<T> GetBuilder<T>() where T : class, IEntity, new() {
			//if T is child of IEnemyEntity, return EnemyBuilder<T>
			//else return BasicEntityBuilder<T>
			
			/*if (typeof(T).IsSubclassOf(typeof(IEnemyEntity))) {
				return EnemyBuilder<T>.Allocate();
			}*/
			

			return BasicEntityBuilder<T>.Allocate();
		}
	}
}