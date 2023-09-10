﻿using Framework;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model.Builders;

namespace Runtime.Enemies.Model {
	/// <summary>
	/// Model for enemies
	/// </summary>
	public interface IEnemyEntityModel : IEntityModel<IEnemyEntity>, ISavableModel {
		/// <summary>
		/// Get the enemy builder for the entity type
		/// </summary>
		/// <param name="rarity"></param>
		/// <param name="addToModelOnceBuilt"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		EnemyBuilder<T> GetEnemyBuilder<T>(int rarity, bool addToModelOnceBuilt = true)
			where T : class, IEnemyEntity, new();
		
	}
	
	
	public class EnemyEntityModel : EntityModel<IEnemyEntity>, IEnemyEntityModel {
		public EnemyBuilder<T> GetEnemyBuilder<T>(int rarity, bool addToModelOnceBuilt = true) where T : class, IEnemyEntity, new() {
			EnemyBuilder<T> builder = entityBuilderFactory.GetBuilder<EnemyBuilder<T>, T>(rarity);
		
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}
	}
}