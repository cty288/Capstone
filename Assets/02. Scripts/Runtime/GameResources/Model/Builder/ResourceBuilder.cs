using System;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.TagProperty;
using Runtime.GameResources.Model.Properties.BaitAdjectives;


namespace Runtime.GameResources.Model.Builder {
	
	public abstract class ResourceBuilder<TBuilder, TEntity> : EntityBuilder<TBuilder, TEntity>
		where TEntity : class, IEntity, new() 
		where TBuilder : ResourceBuilder<TBuilder, TEntity>{
		
		public TBuilder AddBaitAdjective(BaitAdjective adjective) {
			CheckEntity();
			if (Entity.HasProperty(new PropertyNameInfo(PropertyName.bait_adjectives))) {
				Entity.GetProperty<IBaitAdjectives>().BaseValue.Add(adjective);
			}
			return (TBuilder) this;
		}
		
		[Obsolete("This method is obsolete. To set modifier, do this in the entity's own class.")]
		public TBuilder SetMaxStack(int maxStack, IPropertyDependencyModifier<int> modifier = null) {
			CheckEntity();
			SetProperty(new PropertyNameInfo(PropertyName.max_stack), maxStack, modifier);
			return (TBuilder) this;
		}
	}
}