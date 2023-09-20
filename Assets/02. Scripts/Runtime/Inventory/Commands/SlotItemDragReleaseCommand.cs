
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using UnityEngine;

namespace Runtime.Inventory.Commands {
	public class SlotItemDragReleaseCommand : AbstractCommand<SlotItemDragReleaseCommand> {
		
		private Vector2 mousePosition;
		private ResourceSlot fromSlot;
		public SlotItemDragReleaseCommand(){}
		
		
		protected override void OnExecute() {
			ResourceSlot currentHoveredSlot = ResourceSlot.currentHoveredSlot.Slot;
			
			if (currentHoveredSlot != null && currentHoveredSlot != fromSlot) {
				if (currentHoveredSlot != fromSlot) {
					if (currentHoveredSlot is not RubbishSlot) {
						IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(fromSlot.GetLastItemUUID());
						currentHoveredSlot.TryMoveAllItemFromSlot(fromSlot, topItem);
					}else {
						this.SendCommand<PlayerThrowAllSlotResourceCommand>(
							PlayerThrowAllSlotResourceCommand.Allocate(fromSlot));
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