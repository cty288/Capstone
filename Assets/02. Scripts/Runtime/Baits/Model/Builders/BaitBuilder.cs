using System.Collections.Generic;
using System.Linq;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;
using Runtime.Weapons.Model.Builders;

namespace _02._Scripts.Runtime.Baits.Model.Builders {
	public class BaitBuilder<T> : EntityBuilder<BaitBuilder<T>, T> where T : class, IEntity, new() {
		public BaitBuilder() {
			CheckEntity();
		}
		public override void RecycleToCache() {
			SafeObjectPool<BaitBuilder<T>>.Singleton.Recycle(this);
		}
		
		public BaitBuilder<T> SetBaseVigiliance(float baseVigiliance)
		{
			SetProperty<float>(new PropertyNameInfo(PropertyName.vigiliance), baseVigiliance);
			return this;
		}
		
		public BaitBuilder<T> SetBaseTastes(List<TasteType> tastes) {
			SetProperty<List<TasteType>>(new PropertyNameInfo(PropertyName.taste), tastes);
			return this;
		}
		
		
		public static BaitBuilder<T> Allocate(int rarity) {
			BaitBuilder<T> target = SafeObjectPool<BaitBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}