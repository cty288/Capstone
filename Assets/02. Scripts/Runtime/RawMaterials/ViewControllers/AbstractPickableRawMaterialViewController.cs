using System;
using MikroFramework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.ViewControllers;
using Runtime.RawMaterials.Model.Base;
using Runtime.RawMaterials.Model.Builder;
using UnityEngine;

namespace Runtime.RawMaterials.ViewControllers {
	public abstract class AbstractPickableRawMaterialViewController<T> : AbstractPickableResourceViewController<T> 
		where  T : class, IRawMaterialEntity, new(){
		
		protected IRawMaterialModel rawMaterialModel;
		protected Collider[] selfColliders;
		protected override void Awake() {
			base.Awake();
			rawMaterialModel = this.GetModel<IRawMaterialModel>();
			selfColliders = GetComponents<Collider>();
		}

		protected override IEntity OnBuildNewEntity() {
			RawMaterialBuilder<T> builder = rawMaterialModel.GetRawMaterialBuilder<T>();
			return OnInitResourceEntity(builder);
		}

		protected abstract IEntity OnInitResourceEntity(RawMaterialBuilder<T> builder);

		protected override void OnStartAbsorb() {
			foreach (Collider selfCollider in selfColliders) {
				selfCollider.isTrigger = true;
			}
		}

		public override void OnRecycled() {
			base.OnRecycled();
			foreach (Collider selfCollider in selfColliders) {
				selfCollider.isTrigger = false;
			}
		}
	}
}