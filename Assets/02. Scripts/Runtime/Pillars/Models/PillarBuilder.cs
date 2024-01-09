using _02._Scripts.Runtime.Currency.Model;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;
using Runtime.Spawning;

namespace _02._Scripts.Runtime.Pillars.Models {
	public class PillarBuilder<T> : EntityBuilder<PillarBuilder<T>, T> where T : class, IEntity, new()
	{
		public PillarBuilder()
		{
			CheckEntity();
		}
        
		public override void RecycleToCache() {
			SafeObjectPool<PillarBuilder<T>>.Singleton.Recycle(this);
		}

		public static PillarBuilder<T> Allocate(int rarity) {
			PillarBuilder<T> target = SafeObjectPool<PillarBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
		
		public PillarBuilder<T> SetPillarCurrencyType(CurrencyType currencyType) {
			CheckEntity();
			(Entity as IPillarEntity).PillarCurrencyType = currencyType;
			return this;
		}
		
		public PillarBuilder<T> SetRewardCost(RewardCostInfo costInfo) {
			CheckEntity();
			(Entity as IPillarEntity).RewardCost = costInfo;
			return this;
		}
	}
}