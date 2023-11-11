using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.Skills.Model.Builders {
	public class SkillBuilder<T> : EntityBuilder<SkillBuilder<T>, T> where T : class, IEntity, new() {
		public override void RecycleToCache() {
			SafeObjectPool<SkillBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static SkillBuilder<T> Allocate(int rarity)
		{
			SkillBuilder<T> target = SafeObjectPool<SkillBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}