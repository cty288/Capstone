using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;
using Runtime.Weapons.Model.Builders;

namespace _02._Scripts.Runtime.Levels.Models {
	public class LevelBuilder<T> : EntityBuilder<LevelBuilder<T>, T> where T : class, IEntity, new() {

		public LevelBuilder() {
			CheckEntity();
		}
		
		public override void RecycleToCache() {
			SafeObjectPool<LevelBuilder<T>>.Singleton.Recycle(this);
		}

		public static LevelBuilder<T> Allocate(int rarity)
		{
			LevelBuilder<T> target = SafeObjectPool<LevelBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}