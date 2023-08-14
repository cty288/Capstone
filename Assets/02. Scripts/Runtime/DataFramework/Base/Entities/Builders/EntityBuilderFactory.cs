using Runtime.DataFramework.Entities.Enemies;
using UnityEngine;

namespace Runtime.DataFramework.Entities.Builders {
	public interface IEntityBuilderFactory {
		TBuilder GetBuilder<TBuilder, TEntity>(int rarity)
			where TBuilder : EntityBuilder<TBuilder, TEntity>
			where TEntity : class, IEntity, new();
		
	}
	
	public class EntityBuilderFactory : IEntityBuilderFactory {
		
		public TBuilder GetBuilder<TBuilder, TEntity>(int rarity) 
			where TBuilder : EntityBuilder<TBuilder, TEntity> 
			where TEntity : class, IEntity, new() {
			
			if(typeof(TBuilder) == typeof(EnemyBuilder<TEntity>)){
				if (!typeof(IEnemyEntity).IsAssignableFrom(typeof(TEntity))) {
					throw new UnityException("EnemyBuilder can only be used to build IEnemyEntity");
				}
				return EnemyBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}
			
			return BasicEntityBuilder<TEntity>.Allocate(rarity) as TBuilder;
		}
	}
}