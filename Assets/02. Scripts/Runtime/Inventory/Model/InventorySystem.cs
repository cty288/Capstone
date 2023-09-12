using System;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	/*public struct OnInventoryReloadEvent {
		public List<InventorySlotInfo> InventorySlots;
	}*/

	
	public class InventorySystem : AbstractSystem, IInventorySystem {
		private IInventoryModel inventoryModel;
		

		public static int InitialSlotCount = 9;
		public static Dictionary<HotBarCategory, int> InitialHotBarSlotCount = new Dictionary<HotBarCategory, int>() {
			{HotBarCategory.Right, 2},
			{HotBarCategory.Left, 3}
		};
		
		protected override void OnInit() {
			inventoryModel = this.GetModel<IInventoryModel>();

			if (inventoryModel.IsFirstTimeCreated) {
				ResetInventory();
			}
		}
		


		private List<IResourceEntity> GetResourcesByIDs(List<string> uuids) {
			List<IResourceEntity> resources = new List<IResourceEntity>();
			foreach (string uuid in uuids) {
				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
				resources.Add(entity);
			}
			return resources;
		}


		public void ResetInventory() {
			inventoryModel.Clear();
			AddInitialSlots();
			inventoryModel.SelectHotBarSlot(HotBarCategory.Left, 0);
			inventoryModel.SelectHotBarSlot(HotBarCategory.Right, 0);
		}
		
		
		private void AddInitialSlots() {
			inventoryModel.AddSlots(InitialSlotCount);

			inventoryModel.AddHotBarSlots(HotBarCategory.Left, InitialHotBarSlotCount[HotBarCategory.Left],
				new LeftHotBarSlot());
				
			inventoryModel.AddHotBarSlots(HotBarCategory.Right, InitialHotBarSlotCount[HotBarCategory.Right],
				new RightHotBarSlot());
		}
	}
}