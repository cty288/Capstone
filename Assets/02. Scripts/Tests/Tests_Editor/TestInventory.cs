using Framework;
using NUnit.Framework;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;

namespace Tests.Tests_Editor {
	public class TestInventory {
		private IInventorySystem inventorySystem;
		private IInventoryModel inventoryModel;
		private IRawMaterialModel rawMaterialModel;
		
		[SetUp]
		public void SetUp() {
			inventorySystem = MainGame_Test.Interface.GetSystem<IInventorySystem>();
			inventoryModel = MainGame_Test.Interface.GetModel<IInventoryModel>();
			rawMaterialModel = MainGame_Test.Interface.GetModel<IRawMaterialModel>();
			inventorySystem.ResetInventory();
		}
		
		
		[Test]
		public void AddItem_ShouldWork() {
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool result = inventorySystem.AddItem(item);
			Assert.IsTrue(result);
			Assert.AreEqual(1, inventorySystem.GetSlotCurrentItemCount(0));
		}

		[Test]
		public void AddItem_WhenInventoryFull_ShouldFail() {
			
			for (int i = 0; i < inventorySystem.GetSlotCount(); i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig()
					.Build();
				inventorySystem.AddItemAt(item, i);
			}

			IResourceEntity newItem = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig()
				.Build();
			
			bool result = inventorySystem.AddItemAt(newItem, inventorySystem.GetSlotCount());
			Assert.IsFalse(result);
		}
		
		[Test]
		public void AddSameItem_WhenInventoryFull() {
			
			for (int i = 0; i < inventorySystem.GetSlotCount(); i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig()
					.Build();
				inventorySystem.AddItemAt(item, i);
			}

			IResourceEntity newItem = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig()
				.Build();

			for (int i = 0; i < inventorySystem.GetSlotCount(); i++) {
				Assert.IsTrue(inventorySystem.CanPlaceItem(newItem, i));
			}

			bool result = inventorySystem.AddItem(newItem);
			Assert.IsTrue(result);
			Assert.AreEqual(2, inventorySystem.GetSlotCurrentItemCount(0));
		}
		
		
		[Test]
		public void AddSameItem_UntilStackSizeLimitAndBeyond() {
    
			
			
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventorySystem.AddItemAt(item, 0);
			}
			
			IResourceEntity newItem = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool canPlace = inventorySystem.CanPlaceItem(newItem, 0);
			bool result = inventorySystem.AddItem(newItem);
			
			Assert.IsFalse(canPlace);
			Assert.IsTrue(result);
			Assert.AreEqual(20, inventorySystem.GetSlotCurrentItemCount(0));
		}

		[Test]
		public void AddSameItem_UntilStackSizeLimitAndPlaceInNextSlot() {

			
    
			
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventorySystem.AddItemAt(item, 0);
			}
			
			IResourceEntity item2 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool result = inventorySystem.AddItem(item2);
    
			Assert.IsTrue(result);
			Assert.AreEqual(20, inventorySystem.GetSlotCurrentItemCount(0));
			Assert.AreEqual(1, inventorySystem.GetSlotCurrentItemCount(1));
		}

		[Test]
		public void AddItem_WhenInventoryCompletelyFull() {

			
			
			for (int i = 0; i < inventorySystem.GetSlotCount(); i++) {
				for (int j = 0; j < 20; j++) {
					IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
					inventorySystem.AddItemAt(item, i);
				}
			}
			
			IResourceEntity item2 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool canPlace = inventorySystem.CanPlaceItem(item2, 0);
			bool result = inventorySystem.AddItem(item2);
			
			Assert.IsFalse(canPlace);
			Assert.IsFalse(result);
		}
		
		[Test]
		public void AddDifferentItems_ToSameStack() {
    
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			
			inventorySystem.AddItemAt(itemA, 0);
			
			bool canPlace = inventorySystem.CanPlaceItem(itemB, 0);
			bool result = inventorySystem.AddItemAt(itemB,0);
			
			Assert.IsFalse(canPlace);
			Assert.IsFalse(result);
			Assert.AreEqual(1, inventorySystem.GetSlotCurrentItemCount(0));
		}
		[Test]
		public void AddDifferentItem_WhenInventoryFullOfSameItem() {

			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			
			for (int i = 0; i < inventorySystem.GetSlotCount(); i++) {
				for (int j = 0; j < 20; j++) {
					IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
					inventorySystem.AddItemAt(itemA, i);
				}
			}

			
			bool canPlace = inventorySystem.CanPlaceItem(itemB, 0);
			bool result = inventorySystem.AddItem(itemB);

			
			Assert.IsFalse(canPlace);
			Assert.IsFalse(result);
		}
		
		
		[Test]
		public void AddDifferentItem_WhenOneSlotIsOccupiedByAnotherItem() {

			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			
			inventorySystem.AddItemAt(itemA, 0);

			
			bool result = inventorySystem.AddItem(itemB);
			
			Assert.IsTrue(result);
			Assert.AreEqual(1, inventorySystem.GetSlotCurrentItemCount(0)); 
			Assert.AreEqual(1, inventorySystem.GetSlotCurrentItemCount(1)); 
		}
		/*[Test]
		public void RemoveItem_ValidUUID_ShouldWork()
		{
			IResourceEntity item = new SomeResourceEntity(); // 用你的实际类型替换
			inventorySystem.AddItem(item);
			bool result = inventorySystem.RemoveItem(item.UUID);
			Assert.IsTrue(result);
		}

		[Test]
		public void RemoveItem_InvalidUUID_ShouldFail()
		{
			bool result = inventorySystem.RemoveItem("InvalidUUID");
			Assert.IsFalse(result);
		}

		[Test]
		public void AddSlots_WithinLimit_ShouldWork()
		{
			bool result = inventorySystem.AddSlots(5);
			Assert.IsTrue(result);
		}

		[Test]
		public void AddSlots_ExceedLimit_ShouldFail()
		{
			bool result = inventorySystem.AddSlots(InventoryModel.MaxSlotCount + 1);
			Assert.IsFalse(result);
		}*/

	}
}