using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Inventory.Model;

namespace Runtime.Inventory.Commands {
	public class NavigateSelectHotBarSlotCommand: AbstractCommand<NavigateSelectHotBarSlotCommand> {
		private HotBarCategory category;
		private bool isNext;
		
		public NavigateSelectHotBarSlotCommand() {
			
		}
		
		
		public static NavigateSelectHotBarSlotCommand Allocate(HotBarCategory category, bool isNext) {
			NavigateSelectHotBarSlotCommand command = SafeObjectPool<NavigateSelectHotBarSlotCommand>.Singleton.Allocate();
			command.category = category;
			command.isNext = isNext;
			return command;
		}

		protected override void OnExecute() {
			IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
			if (category != HotBarCategory.Right) {
				if (isNext) {
					inventoryModel.SelectNextHotBarSlot(category);
				}
				else {
					inventoryModel.SelectPreviousHotBarSlot(category);
				}
			}
			else {
				List<ResourceSlot> slots = inventoryModel.GetHotBarSlots(HotBarCategory.Right);
				if (isNext) {
					for (int i = slots.Count - 1; i > 0; i--) {
						ResourceSlot.SwapSlotItems(slots[i], slots[i - 1]);
					}
				}
				else {
					for (int i = 0; i < slots.Count - 1; i++) {
						ResourceSlot.SwapSlotItems(slots[i], slots[i + 1]);
					}
				}
			}

		}
	}
}