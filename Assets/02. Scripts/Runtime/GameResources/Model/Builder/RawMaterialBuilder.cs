using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;


namespace Runtime.GameResources.Model.Builder {
	public class RawMaterialBuilder<T> : ResourceBuilder<RawMaterialBuilder<T>, T> where T : class, IEntity, new()  {
		public override void RecycleToCache() {
			SafeObjectPool<RawMaterialBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static RawMaterialBuilder<T> Allocate(int rarity) {
			RawMaterialBuilder<T> target = SafeObjectPool<RawMaterialBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}