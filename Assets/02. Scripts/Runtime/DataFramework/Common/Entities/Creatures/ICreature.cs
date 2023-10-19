using System.Collections.Generic;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model.Properties;

namespace Runtime.DataFramework.Entities.Creatures {
	
	
	public interface ICreature : IDamageable, IHaveCustomProperties, IHaveTags {
		public ItemDropInfo GetRandomDropItem(int rarity);
	}
}