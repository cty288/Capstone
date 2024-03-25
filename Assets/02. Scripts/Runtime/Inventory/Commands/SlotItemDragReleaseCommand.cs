
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
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
			if (!ResourceSlot.currentHoveredSlot.Value)
			{
				AudioSystem.Singleton.Play2DSound("item_invalid");
				return;
			}
			ResourceSlot currentHoveredSlot = ResourceSlot.currentHoveredSlot.Value.Slot;
			IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
			
			if (currentHoveredSlot != null && currentHoveredSlot != fromSlot) {
				if (currentHoveredSlot != fromSlot) {
					if (currentHoveredSlot is RubbishSlot) {
						IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(fromSlot.GetLastItemUUID());
						if (fromSlot.GetCanThrow(topItem)) {
							AudioSystem.Singleton.Play2DSound("discard");
							this.SendCommand<PlayerThrowAllSlotResourceCommand>(
								PlayerThrowAllSlotResourceCommand.Allocate(fromSlot));
						}
						else
						{
							AudioSystem.Singleton.Play2DSound("item_invalid");
						}
					}else if (currentHoveredSlot is UpgradeSlot) {
						ISkillEntity topItem = GlobalGameResourceEntities.GetAnyResource(fromSlot.GetLastItemUUID()) as ISkillEntity;
						if (ResourceSlot.currentHoveredSlot.Value.CanPlaceItem(topItem, false)) {
							AudioSystem.Singleton.Play2DSound("slot_item");
							this.SendCommand<OpenSkillUpgradePanelCommand>(OpenSkillUpgradePanelCommand.Allocate(topItem));
						}
						else
						{
							AudioSystem.Singleton.Play2DSound("item_invalid");
						}
						
					}
					else {
						IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(fromSlot.GetLastItemUUID());
						
						if (currentHoveredSlot.TryMoveAllItemFromSlot(fromSlot, topItem))
						{
							AudioSystem.Singleton.Play2DSound("slot_item");
							topItem.OnInventorySlotUpdate(fromSlot, currentHoveredSlot);
							if (topItem != null && currentHoveredSlot is LeftHotBarSlot slot &&
							    topItem is ISkillEntity skill)
							{
								//skill cooldown reset
								skill.StartSwapInventoryCooldown(swapInventoryCooldown);
							}

							inventorySystem.ForceUpdateCurrentHotBarSlotCanSelect();

						}
						else
						{
							AudioSystem.Singleton.Play2DSound("item_invalid");
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