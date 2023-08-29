using System;
using MikroFramework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.ViewControllers;
using Runtime.RawMaterials.Model.Base;
using Runtime.RawMaterials.Model.Builder;

namespace Runtime.RawMaterials.ViewControllers {
	public abstract class AbstractRawMaterialViewController<T> : AbstractResourceViewController<T> 
		where  T : class, IRawMaterialEntity, new(){
		
		protected IRawMaterialModel rawMaterialModel;

		private void Awake() {
			rawMaterialModel = this.GetModel<IRawMaterialModel>();
		}

		protected override IEntity OnBuildNewEntity() {
			RawMaterialBuilder<T> builder = rawMaterialModel.GetRawMaterialBuilder<T>();
			return OnInitResourceEntity(builder);
		}

		protected abstract IEntity OnInitResourceEntity(RawMaterialBuilder<T> builder);
	}
}