using MikroFramework;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.ViewControllers;
using Runtime.RawMaterials.Model.Base;
using Runtime.RawMaterials.Model.Builder;

namespace Runtime.RawMaterials.ViewControllers {
	public abstract class AbstractRawMaterialViewController<T> : AbstractResourceViewController<T, IRawMaterialModel> 
		where  T : class, IRawMaterialEntity, new(){
		
		protected override IEntity OnInitEntity() {
			RawMaterialBuilder<T> builder = entityModel.GetRawMaterialBuilder<T>();
			return OnInitResourceEntity(builder);
		}

		protected abstract IEntity OnInitResourceEntity(RawMaterialBuilder<T> builder);
	}
}