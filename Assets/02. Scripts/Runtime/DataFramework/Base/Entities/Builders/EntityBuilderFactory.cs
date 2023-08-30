using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.GameResources.Model.Builder;
using Runtime.RawMaterials.Model.Builder;
using Runtime.Weapons.Model.Builders;
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
			}else if (typeof(TBuilder) == typeof(RawMaterialBuilder<TEntity>)) {
				return RawMaterialBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}else if (typeof(TBuilder) == typeof(WeaponBuilder<TEntity>)) {
				return WeaponBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}
			
			return BasicEntityBuilder<TEntity>.Allocate(rarity) as TBuilder;
		}
	}
}