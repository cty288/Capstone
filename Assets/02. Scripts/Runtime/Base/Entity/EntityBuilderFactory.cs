namespace _02._Scripts.Runtime.Base.Entity {
	public interface IEntityBuilderFactory {
		EntityBuilder<T> GetBuilder<T>() where T : class, IEntity, new();
	}
	
	public class EntityBuilderFactory : IEntityBuilderFactory {

		public EntityBuilder<T> GetBuilder<T>() where T : class, IEntity, new() {
			if (typeof(T) == typeof(Player)) { //temp
				return null;
			}

			return BasicEntityBuilder<T>.Allocate();
		}
	}
}