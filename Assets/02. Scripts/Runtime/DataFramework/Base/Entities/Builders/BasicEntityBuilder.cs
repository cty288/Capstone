using _02._Scripts.DataFramework.Base.Properties;
using MikroFramework.Pool;

namespace _02._Scripts.DataFramework.Base.Entities.Builders {
	public class BasicEntityBuilder<T> : EntityBuilder<BasicEntityBuilder<T>, T>  where T : class, IEntity, new() {
		public override void RecycleToCache() {
			SafeObjectPool<BasicEntityBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static BasicEntityBuilder<T> Allocate(int rarity) {
			BasicEntityBuilder<T> target = SafeObjectPool<BasicEntityBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}