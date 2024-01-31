using System;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Builders;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Temporary;
using Runtime.Weapons.Model.Base;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace _02._Scripts.Runtime.WeaponParts.ViewControllers {
	public class CommonPickableWeaponPartsViewController : AbstractPickableResourceViewController<IWeaponPartsEntity> {
		[Header("Build from Inventory")]
		[SerializeField]
		private string entityClassName;
		[SerializeField]
		private int rarity = 1;
		
		
		protected IWeaponPartsModel weaponPartsModel;
		//protected Collider[] selfColliders;
		protected override void Awake() {
			base.Awake();
			weaponPartsModel = this.GetModel<IWeaponPartsModel>();
			//selfColliders = GetComponents<Collider>();
			var players = PlayerController.GetAllPlayers();
			foreach (var player in players) {
				foreach (Collider collider in selfColliders.Keys) {
					Physics.IgnoreCollision(collider, player.transform.Find("Model").GetComponent<Collider>(), true);
				}
			}
		}

		protected override void OnBindEntityProperty() {
			
		}


		protected override void OnStartAbsorb() {
			
		}

		public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity, bool addToModelWhenBuilt = true) {
			if (weaponPartsModel == null) {
				weaponPartsModel = this.GetModel<IWeaponPartsModel>();
			}
			
			IEntityBuilder builder =
				weaponPartsModel.GetWeaponPartsBuilder(entityClassName, addToModelWhenBuilt);
			
			if (setRarity) {
				builder?.SetProperty(new PropertyNameInfo(PropertyName.rarity), rarity);
			}
			
			

			return builder?.SetProperty(new PropertyNameInfo(PropertyName.max_stack), 1).
				FromConfig().Build() as IResourceEntity;
		}

		protected override IEntity OnBuildNewEntity() {
			return OnBuildNewPickableResourceEntity(true, rarity);
		}
	}
}