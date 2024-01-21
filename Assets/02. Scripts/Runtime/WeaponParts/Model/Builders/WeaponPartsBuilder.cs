using _02._Scripts.Runtime.Skills.Model.Builders;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.WeaponParts.Model.Builders {
	
	public class WeaponPartsBuilder<T> : EntityBuilder<WeaponPartsBuilder<T>, T> where T : class, IEntity, new() {
		public override void RecycleToCache() {
			SafeObjectPool<WeaponPartsBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static WeaponPartsBuilder<T> Allocate(int rarity) {
			WeaponPartsBuilder<T> target = SafeObjectPool<WeaponPartsBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}