using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Model.Builder;

namespace Runtime.GameResources.ViewControllers {
	public abstract class AbstractRawMaterialViewController<T> : AbstractResourceViewController<T, IGameResourceModel> 
		where  T : class, IRawMaterialEntity, new(){
		
		protected override IEntity OnInitEntity() {
			RawMaterialBuilder<T> builder = entityModel.GetRawMaterialBuilder<T>();
			return OnInitResourceEntity(builder);
		}

		protected abstract IEntity OnInitResourceEntity(RawMaterialBuilder<T> builder);
	}
}