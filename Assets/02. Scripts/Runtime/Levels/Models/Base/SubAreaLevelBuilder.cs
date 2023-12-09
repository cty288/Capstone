using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;
using Runtime.Weapons.Model.Builders;

namespace _02._Scripts.Runtime.Levels.Models {
	public class SubAreaLevelBuilder<T> : EntityBuilder<SubAreaLevelBuilder<T>, T> where T : class, IEntity, new() {

		public SubAreaLevelBuilder() {
			CheckEntity();
		}
		
		public override void RecycleToCache() {
			SafeObjectPool<SubAreaLevelBuilder<T>>.Singleton.Recycle(this);
		}

		public static SubAreaLevelBuilder<T> Allocate(int rarity)
		{
			SubAreaLevelBuilder<T> target = SafeObjectPool<SubAreaLevelBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}