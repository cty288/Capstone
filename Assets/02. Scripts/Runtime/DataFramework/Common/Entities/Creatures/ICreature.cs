using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;

namespace Runtime.DataFramework.Entities.Creatures {
	
	
	public interface ICreature : IDamagable, IHaveCustomProperties, IHaveTags {
		
	}
}