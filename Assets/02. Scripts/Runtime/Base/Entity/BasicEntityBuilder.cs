using MikroFramework.Pool;

namespace _02._Scripts.Runtime.Base.Entity {
	public class BasicEntityBuilder<T> : EntityBuilder<T>  where T : class, IEntity, new() {
		public override void RecycleToCache() {
			SafeObjectPool<BasicEntityBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static BasicEntityBuilder<T> Allocate(int rarity) {
			BasicEntityBuilder<T> target = SafeObjectPool<BasicEntityBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(PropertyName.rarity, rarity);
			return target;
		}
	}
}