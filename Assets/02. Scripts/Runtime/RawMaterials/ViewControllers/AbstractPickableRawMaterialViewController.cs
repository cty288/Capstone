using System;
using MikroFramework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.RawMaterials.Model.Base;
using Runtime.RawMaterials.Model.Builder;
using Runtime.Temporary;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;


namespace Runtime.RawMaterials.ViewControllers {
	
	public interface IRawMaterialViewController : IResourceViewController {
		IRawMaterialEntity RawMaterialEntity { get; }
	}
	
	public abstract class AbstractPickableRawMaterialViewController<T> : AbstractPickableResourceViewController<T>, IRawMaterialViewController
		where  T : class, IRawMaterialEntity, new(){
		
		protected IRawMaterialModel rawMaterialModel;
		//protected Collider[] selfColliders;
		protected override void Awake() {
			base.Awake();
			rawMaterialModel = this.GetModel<IRawMaterialModel>();
			//selfColliders = GetComponents<Collider>();
			var players = PlayerController.GetAllPlayers();
			foreach (var player in players) {
				foreach (Collider collider in selfColliders.Keys) {
					Physics.IgnoreCollision(collider, player.transform.Find("Model").GetComponent<Collider>(), true);
				}
			}
		}

		
		public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity) {
			RawMaterialBuilder<T> builder = rawMaterialModel.GetRawMaterialBuilder<T>();
			if (setRarity) {
				builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), rarity);
			}
			return OnInitResourceEntity(builder) as IResourceEntity;
		}

		protected abstract IEntity OnInitResourceEntity(RawMaterialBuilder<T> builder);


		public IRawMaterialEntity RawMaterialEntity => BoundEntity;
	}
}