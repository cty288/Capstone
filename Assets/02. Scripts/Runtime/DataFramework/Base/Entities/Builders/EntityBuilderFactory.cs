using _02._Scripts.Runtime.Baits.Model.Builders;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.WeaponParts.Model.Builders;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.GameResources.Model.Builder;
using Runtime.Player.Builders;
using Runtime.RawMaterials.Model.Builder;
using Runtime.Spawning;
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
			}else if (typeof(TBuilder) == typeof(PlayerBuilder<TEntity>)) {
				return PlayerBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}else if (typeof(TBuilder) == typeof(LevelBuilder<TEntity>)) {
				return LevelBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}else if (typeof(TBuilder) == typeof(BaitBuilder<TEntity>)) {
				return BaitBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}else if (typeof(TBuilder) == typeof(DirectorBuilder<TEntity>)) {
				return DirectorBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}else if (typeof(TBuilder) == typeof(SkillBuilder<TEntity>)) {
				return SkillBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}else if (typeof(TBuilder) == typeof(SubAreaLevelBuilder<TEntity>)) {
				return SubAreaLevelBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}else if (typeof(TBuilder) == typeof(WeaponPartsBuilder<TEntity>)) {
				return WeaponPartsBuilder<TEntity>.Allocate(rarity) as TBuilder;
			}

			return BasicEntityBuilder<TEntity>.Allocate(rarity) as TBuilder;
		}
	}
}