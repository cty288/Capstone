using Framework;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.Spawning;

namespace _02._Scripts.Runtime.Pillars.Models {
	public interface IPillarModel : IEntityModel, ISavableModel {
		PillarBuilder<T> GetPillarBuilder<T>(bool addToModelOnceBuilt = true)
			where T : class, IPillarEntity, new();
	}
	
	public class PillarModel: EntityModel<IPillarEntity>, IPillarModel {
		
		public PillarBuilder<T> GetPillarBuilder<T>(bool addToModelOnceBuilt = true) where T : class, IPillarEntity, new() {
			PillarBuilder<T> builder = entityBuilderFactory.GetBuilder<PillarBuilder<T>, T>(1);

			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}
	}
}