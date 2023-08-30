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
		
		[Test]
		public void RemoveItem_Successfully() {
			
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventorySystem.AddItemAt(item, 0);
			
			bool result = inventorySystem.RemoveItem(item.UUID);
    
			Assert.IsTrue(result);
			Assert.AreEqual(0, inventorySystem.GetSlotCurrentItemCount(0));
		}
		
		[Test]
		public void RemoveItemAt_Successfully() {
			
			IResourceEntity item1 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity item2 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
    
			inventorySystem.AddItemAt(item1, 0);
			inventorySystem.AddItemAt(item2, 1);

			
			bool result = inventorySystem.RemoveItemAt(0, item1.UUID);
    
			Assert.IsTrue(result);
			Assert.AreEqual(0, inventorySystem.GetSlotCurrentItemCount(0));
			Assert.AreEqual(1, inventorySystem.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void AddSlots_Successfully() {
			int initialSlotCount = inventorySystem.GetSlotCount();
			
			bool result = inventorySystem.AddSlots(5);
    
			Assert.IsTrue(result);
			Assert.AreEqual(initialSlotCount + 5, inventorySystem.GetSlotCount());
		}
		
		[Test]
		public void ResetInventory_Successfully() {
			inventorySystem.AddSlots(5);
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventorySystem.AddItemAt(item, 0);
    
			// 重置库存
			inventorySystem.ResetInventory();

			Assert.AreEqual(0, inventorySystem.GetSlotCurrentItemCount(0));
			Assert.AreEqual(InventoryModel.InitialSlotCount, inventorySystem.GetSlotCount());
		}

		[Test]
		public void RemoveItem_NonexistentItem() {
			bool result = inventorySystem.RemoveItem("fake_uuid");
			bool result2 = inventorySystem.RemoveItemAt(0, "fake_uuid");
			bool result3 = inventorySystem.RemoveLastItemAt(0);
			Assert.IsFalse(result);
			Assert.IsFalse(result2);
			Assert.IsFalse(result3);
		}
		
		[Test]
		public void RemoveMultipleItemsFromFullSlot() {
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventorySystem.AddItemAt(item, 0);
			}
			
			for (int i = 0; i < 10; i++) {
				inventorySystem.RemoveLastItemAt(0);
			}
    
			Assert.AreEqual(10, inventorySystem.GetSlotCurrentItemCount(0));
		}
		
		
		[Test]
		public void RemoveDifferentTypesOfItems() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventorySystem.AddItemAt(itemA, 0);
			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			inventorySystem.AddItemAt(itemB, 1);

		
			bool resultA = inventorySystem.RemoveItem(itemA.UUID);
			bool resultB = inventorySystem.RemoveItem(itemB.UUID);

			Assert.IsTrue(resultA);
			Assert.IsTrue(resultB);
			Assert.AreEqual(0, inventorySystem.GetSlotCurrentItemCount(0));
			Assert.AreEqual(0, inventorySystem.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void RemoveAndCheckForEmptySlot() {
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventorySystem.AddItemAt(item, 0);
			}
			
			for (int i = 0; i < 20; i++) {
				inventorySystem.RemoveLastItemAt(0);
			}
    
			Assert.AreEqual(0, inventorySystem.GetSlotCurrentItemCount(0));
			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			Assert.IsTrue(inventorySystem.CanPlaceItem(itemB, 0));
		}
		
		[Test]
		public void CrossRemoveItems() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventorySystem.AddItemAt(itemA, 0);

			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			inventorySystem.AddItemAt(itemB, 1);
    
			
			bool resultB = inventorySystem.RemoveItem(itemB.UUID);
			bool resultA = inventorySystem.RemoveItem(itemA.UUID);

			Assert.IsTrue(resultB);
			Assert.IsTrue(resultA);
			Assert.AreEqual(0, inventorySystem.GetSlotCurrentItemCount(0));
			Assert.AreEqual(0, inventorySystem.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void RemoveItemWithPriority() {
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventorySystem.AddItemAt(item, 0);
			inventorySystem.AddItemAt(item, 1);

			
			bool result = inventorySystem.RemoveItem(item.UUID);

			Assert.IsTrue(result);
			
			Assert.AreEqual(0, inventorySystem.GetSlotCurrentItemCount(0));
			Assert.AreEqual(1, inventorySystem.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void CheckEmptySlot() {
			InventorySlotInfo info = inventorySystem.GetItemsAt(0);

			Assert.AreEqual(0, info.Items.Count);
			Assert.IsNull(info.TopItem);
			Assert.AreEqual(0, info.SlotIndex);
		}
		
		[Test]
		public void CheckMixedTypeItems() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			inventorySystem.AddItemAt(itemA, 0);
			inventorySystem.AddItemAt(itemB, 0);

			InventorySlotInfo info = inventorySystem.GetItemsAt(0);

			Assert.AreEqual(1, info.Items.Count);
			Assert.AreEqual(itemA, info.TopItem);
			Assert.AreEqual(0, info.SlotIndex);
		}
		
		[Test]
		public void CheckMultipleSlots() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			inventorySystem.AddItemAt(itemA, 0);
			inventorySystem.AddItemAt(itemB, 1);

			InventorySlotInfo info0 = inventorySystem.GetItemsAt(0);
			InventorySlotInfo info1 = inventorySystem.GetItemsAt(1);

			Assert.AreEqual(1, info0.Items.Count);
			Assert.AreEqual(itemA, info0.TopItem);
			Assert.AreEqual(0, info0.SlotIndex);

			Assert.AreEqual(1, info1.Items.Count);
			Assert.AreEqual(itemB, info1.TopItem);
			Assert.AreEqual(1, info1.SlotIndex);
		}
		
		[Test]
		public void CheckAfterRemoval() {
			
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventorySystem.AddItemAt(item, 0);
    
			
			inventorySystem.RemoveLastItemAt(0);

			InventorySlotInfo info = inventorySystem.GetItemsAt(0);

			Assert.AreEqual(0, info.Items.Count);
			Assert.IsNull(info.TopItem);
			Assert.AreEqual(0, info.SlotIndex);
		}


		
	}
}