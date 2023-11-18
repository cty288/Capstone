using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.Model.Base;
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
			IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
			
			if (category != HotBarCategory.Right) {
				ICurrencyModel currencyModel = this.GetModel<ICurrencyModel>();
				IResourceEntity topItem = null;
				
				int index = inventoryModel.GetSelectedHotBarSlotIndex(category);
				int targetIndex = index;
				HotBarSlot slot = null;
				do {
					if (isNext) {
						targetIndex = (targetIndex + 1) % inventoryModel.GetHotBarSlots(category).Count;
					}
					else {
						targetIndex = (targetIndex - 1 + inventoryModel.GetHotBarSlots(category).Count) %
						              inventoryModel.GetHotBarSlots(category).Count;
					}

					slot = inventoryModel.GetHotBarSlots(category)[targetIndex];
					topItem = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
				}while(targetIndex != index && (!slot.IsEmpty() && !slot.GetCanSelect(topItem, currencyModel.GetCurrencyAmountDict())));

				if (targetIndex != index) {
					inventorySystem.SelectHotBarSlot(category, targetIndex);
				}
				
			}
			else {
				List<HotBarSlot> slots = inventoryModel.GetHotBarSlots(HotBarCategory.Right);
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

				inventorySystem.SelectHotBarSlot(HotBarCategory.Right,
					inventoryModel.GetSelectedHotBarSlotIndex(HotBarCategory.Right));
			}

		}
	}
}