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
			//fake code
			EquipToCurrentWeapon();
		}

		private void EquipToCurrentWeapon() {
			IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
			IWeaponEntity weapon = inventorySystem.GetCurrentlySelectedEntity() as IWeaponEntity;

			IWeaponPartsBuff buff = BoundEntity.OnGetBuff(weapon);
			this.GetSystem<IBuffSystem>().AddBuff(weapon, buff);
		}

		public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity) {
			if (weaponPartsModel == null) {
				weaponPartsModel = this.GetModel<IWeaponPartsModel>();
			}
			
			IEntityBuilder builder =
				weaponPartsModel.GetWeaponPartsBuilder(entityClassName);
			
			if (setRarity) {
				builder?.SetProperty(new PropertyNameInfo(PropertyName.rarity), rarity);
			}

			return builder?.FromConfig().Build() as IResourceEntity;
		}
		
		
	}
}