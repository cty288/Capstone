using _02._Scripts.Runtime.Common.Entities.Enemies;

namespace _02._Scripts.Runtime.Base.Entity {
	public interface IEntityBuilderFactory {
		EntityBuilder<T> GetBuilder<T>(int rarity) where T : class, IEntity, new();
		
	}
	
	public class EntityBuilderFactory : IEntityBuilderFactory {

		public EntityBuilder<T> GetBuilder<T>(int rarity) where T : class, IEntity, new() {
			if (typeof(IEnemyEntity).IsAssignableFrom(typeof(T))) {
				return EnemyBuilder<T>.Allocate(rarity);
			}
			return BasicEntityBuilder<T>.Allocate(rarity);
		}
		
		
	}
}