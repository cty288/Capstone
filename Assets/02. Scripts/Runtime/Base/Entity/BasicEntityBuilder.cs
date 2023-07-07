using MikroFramework.Pool;

namespace _02._Scripts.Runtime.Base.Entity {
	public class BasicEntityBuilder<T> : EntityBuilder<T>  where T : class, IEntity, new() {
		public override void RecycleToCache() {
			SafeObjectPool<BasicEntityBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static BasicEntityBuilder<T> Allocate() {
			return SafeObjectPool<BasicEntityBuilder<T>>.Singleton.Allocate();
		}
	}
}