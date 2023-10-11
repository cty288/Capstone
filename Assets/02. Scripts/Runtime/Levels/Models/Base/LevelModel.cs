using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Levels.Models {
	public interface ILevelModel : IEntityModel, IModel {
		BindableProperty<int> CurrentLevelCount { get; }
		
		LevelBuilder<T> GetLevelBuilder<T>(bool addToModelOnceBuilt = true)
			where T : class, ILevelEntity, new();
		
		ILevelEntity CurrentLevel { get; set; }
		
	}
	public class LevelModel: EntityModel<ILevelEntity>, ILevelModel {
		[field: ES3Serializable] public BindableProperty<int> CurrentLevelCount { get; } = new BindableProperty<int>(1);
		public LevelBuilder<T> GetLevelBuilder<T>(bool addToModelOnceBuilt = true) where T : class, ILevelEntity, new() {
			LevelBuilder<T> builder = entityBuilderFactory.GetBuilder<LevelBuilder<T>, T>(1);

			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}

		public ILevelEntity CurrentLevel { get; set; } = null;
	}
}