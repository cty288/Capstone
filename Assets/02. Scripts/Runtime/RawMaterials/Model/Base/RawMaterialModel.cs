using Framework;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Builder;

namespace Runtime.RawMaterials.Model.Base {
	public interface IRawMaterialModel : IGameResourceModel<IRawMaterialEntity>, ISavableModel {
		RawMaterialBuilder<T> GetRawMaterialBuilder<T>(bool addToModelOnceBuilt = true)
			where T : class, IRawMaterialEntity, new();
	}
	
	public class RawMaterialModel : GameResourceModel<IRawMaterialEntity>, IRawMaterialModel {
		public RawMaterialBuilder<T> GetRawMaterialBuilder<T>(bool addToModelOnceBuilt = true) where T : class, IRawMaterialEntity, new() {
			RawMaterialBuilder<T> builder = entityBuilderFactory.GetBuilder<RawMaterialBuilder<T>, T>(1);
		
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}
		
		
	}
}