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
			if (isNext) {
				inventoryModel.SelectNextHotBarSlot(category);
			}
			else {
				inventoryModel.SelectPreviousHotBarSlot(category);
			}
		}
	}
}