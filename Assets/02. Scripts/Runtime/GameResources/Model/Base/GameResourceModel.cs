using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Builder;

namespace Runtime.GameResources.Model.Base {

	public interface IGameResourceModel : IModel, IEntityModel<IResourceEntity> {
		RawMaterialBuilder<T> GetRawMaterialBuilder<T>(bool addToModelOnceBuilt = true)
			where T : class, IRawMaterialEntity, new();
	}
	public class GameResourceModel : EntityModel<IResourceEntity>, IGameResourceModel {
		protected override void OnInit() {
			base.OnInit();
		}

		public RawMaterialBuilder<T> GetRawMaterialBuilder<T>(bool addToModelOnceBuilt = true) where T : class, IRawMaterialEntity, new() {
			RawMaterialBuilder<T> builder = entityBuilderFactory.GetBuilder<RawMaterialBuilder<T>, T>(1);
		
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}
	}
}