using _02._Scripts.Runtime.Scraps.Model.Base;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.GameResources.Model.Builder;
using Runtime.RawMaterials.Model.Builder;

namespace _02._Scripts.Runtime.Scraps.Model.Builder {
	public class ScrapBuilder : ResourceBuilder<ScrapBuilder, ScrapEntity>   {
		public override void RecycleToCache() {
			SafeObjectPool<ScrapBuilder>.Singleton.Recycle(this);
		}
		
		public static ScrapBuilder Allocate(int rarity) {
			ScrapBuilder target = SafeObjectPool<ScrapBuilder>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}