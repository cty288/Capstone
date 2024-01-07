﻿
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace Runtime.Inventory.Commands {
	public class SlotItemDragReleaseCommand : AbstractCommand<SlotItemDragReleaseCommand> {
		
		private Vector2 mousePosition;
		private ResourceSlot fromSlot;
		private float swapInventoryCooldown = 0;

		public SlotItemDragReleaseCommand() {
			swapInventoryCooldown =
				float.Parse(ConfigDatas.Singleton.GlobalDataTable.Get("SWAP_INV_COOLDOWN", "Value1"));
		}
		
		
		protected override void OnExecute() {
			if (!ResourceSlot.currentHoveredSlot) {
				return;
			}
			ResourceSlot currentHoveredSlot = ResourceSlot.currentHoveredSlot.Slot;
			IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
			
			if (currentHoveredSlot != null && currentHoveredSlot != fromSlot) {
				if (currentHoveredSlot != fromSlot) {
					if (currentHoveredSlot is RubbishSlot) {
						IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(fromSlot.GetLastItemUUID());
						if (fromSlot.GetCanThrow(topItem)) {
							this.SendCommand<PlayerThrowAllSlotResourceCommand>(
								PlayerThrowAllSlotResourceCommand.Allocate(fromSlot));
						}
					}else if (currentHoveredSlot is UpgradeSlot) {
						ISkillEntity topItem = GlobalGameResourceEntities.GetAnyResource(fromSlot.GetLastItemUUID()) as ISkillEntity;
						if (ResourceSlot.currentHoveredSlot.CanPlaceItem(topItem, false)) {
							this.SendCommand<OpenSkillUpgradePanelCommand>(OpenSkillUpgradePanelCommand.Allocate(topItem));
						}
						
					}
					else {
						IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(fromSlot.GetLastItemUUID());
						
						if (ResourceSlot.currentHoveredSlot.CanPlaceItem(topItem, false)
						&& currentHoveredSlot.TryMoveAllItemFromSlot(fromSlot, topItem)) {
							if (topItem != null && currentHoveredSlot is LeftHotBarSlot slot && topItem is ISkillEntity skill) {
								//skill cooldown reset
								skill.StartSwapInventoryCooldown(swapInventoryCooldown);
							}
							inventorySystem.ForceUpdateCurrentHotBarSlotCanSelect();
							
						}
					}
				}
			}
		}
		
		
		public static SlotItemDragReleaseCommand Allocate(Vector2 mousePosition, ResourceSlot fromSlot) {
			SlotItemDragReleaseCommand command = SafeObjectPool<SlotItemDragReleaseCommand>.Singleton.Allocate();
			command.mousePosition = mousePosition;
			command.fromSlot = fromSlot;
			return command;
		}
		
		
	}
}