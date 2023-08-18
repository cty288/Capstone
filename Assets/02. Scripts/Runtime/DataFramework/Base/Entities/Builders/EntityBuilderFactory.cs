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
					Debug.LogWarning("EnemyBuilder is designed to be used to ONLY build IEnemyEntity! " +
					                 "If this is not what you want, please use BasicEntityBuilder instead!");
				}
				return EnemyBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}
			
			return BasicEntityBuilder<TEntity>.Allocate(rarity) as TBuilder;
		}
	}
}