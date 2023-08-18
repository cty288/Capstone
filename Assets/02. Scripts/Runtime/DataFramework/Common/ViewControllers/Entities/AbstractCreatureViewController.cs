using DataFramework.Entities;
using DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using DataFramework.Entities.ClassifiedTemplates.Damagable;
using DataFramework.Entities.ClassifiedTemplates.Tags;
using DataFramework.Entities.Creatures;

namespace DataFramework.ViewControllers.Entities {
	
	/// <summary>
	/// An abstract view controller for creature entity (like player, enemy, etc.)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TEntityModel"></typeparam>
	public abstract class AbstractCreatureViewController<T, TEntityModel> : AbstractDamagableViewController<T, TEntityModel>
		where T : class, IHaveCustomProperties, IHaveTags, IDamagable, ICreature, new()
		where TEntityModel: class, IEntityModel {
		
	}
}