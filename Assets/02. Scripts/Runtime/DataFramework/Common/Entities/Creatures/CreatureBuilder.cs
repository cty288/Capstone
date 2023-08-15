using MikroFramework.Pool;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.Enemies;
using Runtime.DataFramework.Properties;

namespace Runtime.DataFramework.Entities.Creatures {
	public abstract class CreatureBuilder<TBuilder, TEntity> : EntityBuilder<TBuilder, TEntity>
		where TEntity : class, IEntity, new() 
		where TBuilder : CreatureBuilder<TBuilder, TEntity>{
		
		
		public TBuilder SetHealth(HealthInfo healthInfo, IPropertyDependencyModifier<HealthInfo> modifier = null) {
			SetProperty<HealthInfo>(new PropertyNameInfo(PropertyName.health), healthInfo, modifier);
			return (TBuilder) this;
		}
		
		public TBuilder SetHealthModifier(IPropertyDependencyModifier<HealthInfo> modifier = null) {
			SetModifier(new PropertyNameInfo(PropertyName.health), modifier);
			return (TBuilder)this;
		}

		public TBuilder SetInvincible(bool isInvincible) {
			CheckEntity();
			((IDamagable) Entity).IsInvincible.Value = isInvincible;
			return (TBuilder) this;
		}
		
	}
	

}