using Framework;
using NUnit.Framework;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;

namespace Tests.Tests_Editor {
	public class TestInventory {
		
		private IInventoryModel inventoryModel;
		//private IInventorySystem inventorySystem;
		private IRawMaterialModel rawMaterialModel;
		private IInventorySystem inventorySystem;
		
		[SetUp]
		public void SetUp() {
			MainGame_Test.ResetArchitecture();
			inventoryModel = MainGame_Test.Interface.GetModel<IInventoryModel>();
			rawMaterialModel = MainGame_Test.Interface.GetModel<IRawMaterialModel>();
			inventorySystem = MainGame_Test.Interface.GetSystem<IInventorySystem>();
			inventorySystem.ResetSlots();
			//inventoryModel.GetAllSlots().Clear();
		}
		
		
		[Test]
		public void AddItem_ShouldWork() {
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool result = inventorySystem.AddItem(item);
			Assert.IsTrue(result);
			Assert.AreEqual(1, inventoryModel.GetAllSlots()[0].GetQuantity());
		}
		
		
		
		[Test]
		public void AddSameItem_UntilStackSizeLimitAndBeyond() {
    
			
			
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventorySystem.AddItem(item);
			}
			
			IResourceEntity newItem = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool canPlace = inventoryModel.CanPlaceItem(newItem);
			bool result = inventorySystem.AddItem(newItem);
			
			Assert.IsTrue(canPlace);
			Assert.IsTrue(result);
		}

		[Test]
		public void AddSameItem_UntilStackSizeLimitAndPlaceInNextSlot() {

			
    
			
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventorySystem.AddItem(item);
			}
			
			IResourceEntity item2 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool result = inventorySystem.AddItem(item2);
    
			Assert.IsTrue(result);
			Assert.AreEqual(20, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(1));
		}

		[Test]
		public void AddItem_WhenInventoryCompletelyFull() {

			
			
			for (int i = 0; i < inventoryModel.GetSlotCount(); i++) {
				for (int j = 0; j < 20; j++) {
					IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
					inventorySystem.AddItem(item);
				}
			}
			
			IResourceEntity item2 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			bool canPlace = inventoryModel.CanPlaceItem(item2);
			bool result = inventorySystem.AddItem(item2);
			
			Assert.IsFalse(canPlace);
			Assert.IsFalse(result);
		}
		
		
		[Test]
		public void AddDifferentItem_WhenInventoryFullOfSameItem() {

			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			
			for (int i = 0; i < inventoryModel.GetSlotCount(); i++) {
				for (int j = 0; j < 20; j++) {
					IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
					inventorySystem.AddItem(itemA);
				}
			}

			
			bool canPlace = inventoryModel.CanPlaceItem(itemB);
			bool result = inventorySystem.AddItem(itemB);

			
			Assert.IsFalse(canPlace);
			Assert.IsFalse(result);
		}
		
		
		[Test]
		public void AddDifferentItem_WhenOneSlotIsOccupiedByAnotherItem() {

			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
    
			
			inventorySystem.AddItem(itemA);

			
			bool result = inventorySystem.AddItem(itemB);
			
			Assert.IsTrue(result);
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(0)); 
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(1)); 
		}
		
		[Test]
		public void RemoveItem_Successfully() {
			
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventorySystem.AddItem(item);
			
			bool result = inventorySystem.RemoveItem(item);
    
			Assert.IsTrue(result);
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
		}
		
		[Test]
		public void RemoveItemAt_Successfully() {
			
			IResourceEntity item1 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			IResourceEntity item2 = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
    
			inventorySystem.AddItem(item1);
			inventorySystem.AddItem(item2);

			
			bool result = inventorySystem.RemoveItem(item1);
    
			Assert.IsTrue(result);
			Assert.AreEqual(1, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void AddSlots_Successfully() {
			int initialSlotCount = inventoryModel.GetSlotCount();

			bool result = inventoryModel.AddSlots(5, out _);
    
			Assert.IsTrue(result);
			Assert.AreEqual(initialSlotCount + 5, inventoryModel.GetSlotCount());
		}
		
		[Test]
		public void ResetInventory_Successfully() {
			inventoryModel.AddSlots(5, out _);
			IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventorySystem.AddItem(item);
    
			// 重置库存
			inventorySystem.ResetSlots();

			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(InventorySystem.InitialSlotCount, inventoryModel.GetSlotCount());
		}

		[Test]
		public void RemoveItem_NonexistentItem() {
			bool result = inventoryModel.RemoveItem("fake_uuid");
			bool result2 = inventoryModel.RemoveItem("fake_uuid");
			
			Assert.IsFalse(result);
			Assert.IsFalse(result2);
			//Assert.IsFalse(result3);
		}
		
		[Test]
		public void RemoveMultipleItemsFromFullSlot() {
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventorySystem.AddItem(item);
			}
			
			for (int i = 0; i < 10; i++) {
				inventoryModel.GetAllSlots()[0].RemoveLastItem();
			}
    
			Assert.AreEqual(10, inventoryModel.GetSlotCurrentItemCount(0));
		}
		
		
		[Test]
		public void RemoveDifferentTypesOfItems() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventoryModel.GetAllSlots()[0].TryAddItem(itemA);
			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			inventoryModel.GetAllSlots()[1].TryAddItem(itemB);

		
			bool resultA = inventorySystem.RemoveItem(itemA);
			bool resultB = inventorySystem.RemoveItem(itemB);

			Assert.IsTrue(resultA);
			Assert.IsTrue(resultB);
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(1));
		}
		
		[Test]
		public void RemoveAndCheckForEmptySlot() {
			for (int i = 0; i < 20; i++) {
				IResourceEntity item = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
				inventoryModel.AddItem(item);
			}
			
			for (int i = 0; i < 20; i++) {
				inventoryModel.GetAllSlots()[0].RemoveLastItem();
			}
    
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			Assert.IsTrue(inventoryModel.CanPlaceItem(itemB));
		}
		
		[Test]
		public void CrossRemoveItems() {
			IResourceEntity itemA = rawMaterialModel.GetRawMaterialBuilder<TestBasicRawMaterial>().FromConfig().Build();
			inventoryModel.AddItem(itemA);

			
			IResourceEntity itemB = rawMaterialModel.GetRawMaterialBuilder<TestEmptyRawMaterial>().FromConfig().Build();
			inventoryModel.AddItem(itemB);
    
			
			bool resultB = inventorySystem.RemoveItem(itemB);
			bool resultA = inventorySystem.RemoveItem(itemA);

			Assert.IsTrue(resultB);
			Assert.IsTrue(resultA);
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(0));
			Assert.AreEqual(0, inventoryModel.GetSlotCurrentItemCount(1));
		}

	}
}