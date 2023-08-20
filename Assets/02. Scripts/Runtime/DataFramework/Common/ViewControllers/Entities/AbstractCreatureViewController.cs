using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;

namespace Runtime.DataFramework.ViewControllers.Entities {
	
	/// <summary>
	/// An abstract view controller for creature entity (like player, enemy, etc.)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TEntityModel"></typeparam>
	public abstract class AbstractCreatureViewController<T, TEntityModel> : AbstractDamagableViewController<T, TEntityModel>
		where T : class, IHaveCustomProperties, IHaveTags, IDamageable, ICreature, new()
		where TEntityModel: class, IEntityModel {
		
	}
}