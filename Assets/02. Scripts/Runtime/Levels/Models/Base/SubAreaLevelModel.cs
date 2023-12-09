using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.Models {
	public interface ISubAreaLevelModel : IEntityModel, IModel {
		SubAreaLevelBuilder<T> GetSubAreaLevelBuilder<T>(bool addToModelOnceBuilt = true)
			where T : class, ISubAreaLevelEntity, new();
	}

	public class SubAreaLevelModel: EntityModel<ISubAreaLevelEntity>, ISubAreaLevelModel {
		// private Dictionary<int, ILevelEntity> levelEntities = new Dictionary<int, ILevelEntity>();
		public SubAreaLevelBuilder<T> GetSubAreaLevelBuilder<T>(bool addToModelOnceBuilt = true) where T : class, ISubAreaLevelEntity, new() {
			SubAreaLevelBuilder<T> builder = entityBuilderFactory.GetBuilder<SubAreaLevelBuilder<T>, T>(1);
		
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}
		
			return builder;
		}
	}
}