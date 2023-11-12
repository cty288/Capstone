using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Inventory.Model;

namespace Runtime.Inventory.Commands {
	public class DirectSelectHotBarSlotCommand : AbstractCommand<DirectSelectHotBarSlotCommand> {
		private HotBarCategory category;
		private int index;
		
		public DirectSelectHotBarSlotCommand() {
			
		}
		
		
		public static DirectSelectHotBarSlotCommand Allocate(HotBarCategory category, int index) {
			DirectSelectHotBarSlotCommand command = SafeObjectPool<DirectSelectHotBarSlotCommand>.Singleton.Allocate();
			command.category = category;
			command.index = index;
			return command;
		}

		protected override void OnExecute() {
			//IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
			IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
			inventorySystem.SelectHotBarSlot(category, index);
		}
	}
}