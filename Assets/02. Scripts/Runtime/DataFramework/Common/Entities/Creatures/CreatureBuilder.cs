using System;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;

namespace Runtime.DataFramework.Entities.Creatures {
	public abstract class CreatureBuilder<TBuilder, TEntity> : EntityBuilder<TBuilder, TEntity>
		where TEntity : class, IEntity, new() 
		where TBuilder : CreatureBuilder<TBuilder, TEntity>{
		
		
		public TBuilder SetHealth(HealthInfo healthInfo) {
			SetProperty<HealthInfo>(new PropertyNameInfo(PropertyName.health), healthInfo);
			return (TBuilder) this;
		}
		
		
		[Obsolete("This method is obsolete. To set modifier, do this in the entity's own class.")]
		public TBuilder SetHealthModifier(IPropertyDependencyModifier<HealthInfo> modifier = null) {
			SetModifier(new PropertyNameInfo(PropertyName.health), modifier);
			return (TBuilder)this;
		}

		public TBuilder SetInvincible(bool isInvincible) {
			CheckEntity();
			((IDamageable) Entity).IsInvincible.Value = isInvincible;
			return (TBuilder) this;
		}
		
	}
	

}