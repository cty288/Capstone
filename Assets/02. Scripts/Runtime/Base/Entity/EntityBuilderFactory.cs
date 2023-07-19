using _02._Scripts.Runtime.Common.Entities.Enemies;
using UnityEngine;

namespace _02._Scripts.Runtime.Base.Entity {
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