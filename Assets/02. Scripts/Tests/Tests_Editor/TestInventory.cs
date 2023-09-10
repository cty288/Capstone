using Framework;
using NUnit.Framework;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;

namespace Tests.Tests_Editor {
	public class TestInventory {
		
		private IInventoryModel inventoryModel;
		private IRawMaterialModel rawMaterialModel;
		private IInventorySystem inventorySystem;
		
		[SetUp]
		public void SetUp() {
			
			inventoryModel = MainGame_Test.Interface.GetModel<IInventoryModel>();
			rawMaterialModel = MainGame_Test.Interface.GetModel<IRawMaterialModel>();
			inventorySystem = MainGame_Test.Interface.GetSystem<IInventorySystem>();
			inventoryModel.Reset();
		}
		
		
		[Test]
		public void AddItem_ShouldWork() {
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool result = inventoryModel.AddItem(item);
			Assert.IsTrue(result);
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(0));
		}

		[Test]
		public void AddItem_WhenInventoryFull_ShouldFail() {
			
			for (int i = 0; i < inventoryModel.GetSlotCount(); i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig()
					.Build();
				inventoryModel.AddItemAt(item, i);
			}

			IResourceEntity newItem = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig()
				.Build();
			
			bool result = inventoryModel.AddItemAt(newItem, inventoryModel.GetSlotCount());
			Assert.IsFalse(result);
		}
		
		[Test]
		public void AddSameItem_WhenInventoryFull() {
			
			for (int i = 0; i < inventoryModel.GetSlotCount(); i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig()
					.Build();
				inventoryModel.AddItemAt(item, i);
			}

			IResourceEntity newItem = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig()
				.Build();

			for (int i = 0; i < inventoryModel.GetSlotCount(); i++) {
				Assert.IsTrue(inventoryModel.CanPlaceItem(newItem, i));
			}

			bool result = inventoryModel.AddItem(newItem);
			Assert.IsTrue(result);
			Assert.AreEqual(2, inventoryModel.GetSlotCurrentItemCount(0));
		}
		
		
		[Test]
		public void AddSameItem_UntilStackSizeLimitAndBeyond() {
    
			
			
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventoryModel.AddItemAt(item, 0);
			}
			
			IResourceEntity newItem = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool canPlace = inventoryModel.CanPlaceItem(newItem, 0);
			bool result = inventoryModel.AddItem(newItem);
			
			Assert.IsFalse(canPlace);
			Assert.IsTrue(result);
			Assert.AreEqual(20, inventoryModel.GetSlotCurrentItemCount(0));
		}

		[Test]
		public void AddSameItem_UntilStackSizeLimitAndPlaceInNextSlot() {

			
    
			
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventoryModel.AddItemAt(item, 0);
			}
			
			IResourceEntity item2 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool result = inventoryModel.AddItem(item2);
    
			Assert.IsTrue(result);
			Assert.AreEqual(20, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(1));
		}

		[Test]
		public void AddItem_WhenInventoryCompletelyFull() {

			
			
			for (int i = 0; i < inventoryModel.GetSlotCount(); i++) {
				for (int j = 0; j < 20; j++) {
					IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
					inventoryModel.AddItemAt(item, i);
				}
			}
			
			IResourceEntity item2 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool canPlace = inventoryModel.CanPlaceItem(item2, 0);
			bool result = inventoryModel.AddItem(item2);
			
			Assert.IsFalse(canPlace);
			Assert.IsFalse(result);
		}
		
		[Test]
		public void AddDifferentItems_ToSameStack() {
    
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			
			inventoryModel.AddItemAt(itemA, 0);
			
			bool canPlace = inventoryModel.CanPlaceItem(itemB, 0);
			bool result = inventoryModel.AddItemAt(itemB,0);
			
			Assert.IsFalse(canPlace);
			Assert.IsFalse(result);
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(0));
		}
		[Test]
		public void AddDifferentItem_WhenInventoryFullOfSameItem() {

			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			
			for (int i = 0; i < inventoryModel.GetSlotCount(); i++) {
				for (int j = 0; j < 20; j++) {
					IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
					inventoryModel.AddItemAt(itemA, i);
				}
			}

			
			bool canPlace = inventoryModel.CanPlaceItem(itemB, 0);
			bool result = inventoryModel.AddItem(itemB);

			
			Assert.IsFalse(canPlace);
			Assert.IsFalse(result);
		}
		
		
		[Test]
		public void AddDifferentItem_WhenOneSlotIsOccupiedByAnotherItem() {

			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			
			inventoryModel.AddItemAt(itemA, 0);

			
			bool result = inventoryModel.AddItem(itemB);
			
			Assert.IsTrue(result);
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(0)); 
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(1)); 
		}
		
		[Test]
		public void RemoveItem_Successfully() {
			
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventoryModel.AddItemAt(item, 0);
			
			bool result = inventoryModel.RemoveItem(item.UUID);
    
			Assert.IsTrue(result);
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
		}
		
		[Test]
		public void RemoveItemAt_Successfully() {
			
			IResourceEntity item1 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity item2 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
    
			inventoryModel.AddItemAt(item1, 0);
			inventoryModel.AddItemAt(item2, 1);

			
			bool result = inventoryModel.RemoveItemAt(0, item1.UUID);
    
			Assert.IsTrue(result);
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void AddSlots_Successfully() {
			int initialSlotCount = inventoryModel.GetSlotCount();
			
			bool result = inventoryModel.AddSlots(5);
    
			Assert.IsTrue(result);
			Assert.AreEqual(initialSlotCount + 5, inventoryModel.GetSlotCount());
		}
		
		[Test]
		public void ResetInventory_Successfully() {
			inventoryModel.AddSlots(5);
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventoryModel.AddItemAt(item, 0);
    
			// 重置库存
			inventoryModel.Reset();

			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(InventoryModel.InitialSlotCount, inventoryModel.GetSlotCount());
		}

		[Test]
		public void RemoveItem_NonexistentItem() {
			bool result = inventoryModel.RemoveItem("fake_uuid");
			bool result2 = inventoryModel.RemoveItemAt(0, "fake_uuid");
			bool result3 = inventoryModel.RemoveLastItemAt(0);
			Assert.IsFalse(result);
			Assert.IsFalse(result2);
			Assert.IsFalse(result3);
		}
		
		[Test]
		public void RemoveMultipleItemsFromFullSlot() {
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventoryModel.AddItemAt(item, 0);
			}
			
			for (int i = 0; i < 10; i++) {
				inventoryModel.RemoveLastItemAt(0);
			}
    
			Assert.AreEqual(10, inventoryModel.GetSlotCurrentItemCount(0));
		}
		
		
		[Test]
		public void RemoveDifferentTypesOfItems() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventoryModel.AddItemAt(itemA, 0);
			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			inventoryModel.AddItemAt(itemB, 1);

		
			bool resultA = inventoryModel.RemoveItem(itemA.UUID);
			bool resultB = inventoryModel.RemoveItem(itemB.UUID);

			Assert.IsTrue(resultA);
			Assert.IsTrue(resultB);
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void RemoveAndCheckForEmptySlot() {
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventoryModel.AddItemAt(item, 0);
			}
			
			for (int i = 0; i < 20; i++) {
				inventoryModel.RemoveLastItemAt(0);
			}
    
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			Assert.IsTrue(inventoryModel.CanPlaceItem(itemB, 0));
		}
		
		[Test]
		public void CrossRemoveItems() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventoryModel.AddItemAt(itemA, 0);

			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			inventoryModel.AddItemAt(itemB, 1);
    
			
			bool resultB = inventoryModel.RemoveItem(itemB.UUID);
			bool resultA = inventoryModel.RemoveItem(itemA.UUID);

			Assert.IsTrue(resultB);
			Assert.IsTrue(resultA);
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void RemoveItemWithPriority() {
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventoryModel.AddItemAt(item, 0);
			inventoryModel.AddItemAt(item, 1);

			
			bool result = inventoryModel.RemoveItem(item.UUID);

			Assert.IsTrue(result);
			
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void CheckEmptySlot() {
			SlotInfo info = inventorySystem.GetItemsAt(0);

			Assert.AreEqual(0, info.Items.Count);
			Assert.IsNull(info.TopItem);
			Assert.AreEqual(0, info.SlotIndex);
		}
		
		[Test]
		public void CheckMixedTypeItems() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			inventoryModel.AddItemAt(itemA, 0);
			inventoryModel.AddItemAt(itemB, 0);

			SlotInfo info = inventorySystem.GetItemsAt(0);

			Assert.AreEqual(1, info.Items.Count);
			Assert.AreEqual(itemA, info.TopItem);
			Assert.AreEqual(0, info.SlotIndex);
		}
		
		[Test]
		public void CheckMultipleSlots() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			inventoryModel.AddItemAt(itemA, 0);
			inventoryModel.AddItemAt(itemB, 1);

			SlotInfo info0 = inventorySystem.GetItemsAt(0);
			SlotInfo info1 = inventorySystem.GetItemsAt(1);

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
			inventoryModel.AddItemAt(item, 0);
    
			
			inventoryModel.RemoveLastItemAt(0);

			SlotInfo info = inventorySystem.GetItemsAt(0);

			Assert.AreEqual(0, info.Items.Count);
			Assert.IsNull(info.TopItem);
			Assert.AreEqual(0, info.SlotIndex);
		}


		
	}
}