using System.Collections.Generic;
using System.Linq;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model.Properties;

namespace Runtime.DataFramework.Entities.Creatures {
	public abstract class AbstractCreature : DamageableEntity, ICreature {
		protected IItemDropCollectionsProperty itemDropCollectionsProperty;
		public override void OnAwake() {
			base.OnAwake();
			itemDropCollectionsProperty = GetProperty<IItemDropCollectionsProperty>();
		}

		public ItemDropInfo GetRandomDropItem() {
			Dictionary<int, ItemDropCollection> itemDropCollections = itemDropCollectionsProperty.RealValues.Value;
			
			if(itemDropCollections == null || itemDropCollections.Count == 0) {
				return default;
			}


			ItemDropCollection itemDropCollection = GetItemDropCollection();
			
			ItemDropInfo[] itemDropInfos = itemDropCollection.ItemDropInfos;
			if (itemDropInfos == null || itemDropInfos.Length == 0) {
				return default;
			}
			
			float totalWeight = itemDropInfos.Sum(info => info.required? 0 : info.dropWeight);
			float randomWeight = UnityEngine.Random.Range(0, totalWeight);
			float currentWeight = 0;
			foreach (ItemDropInfo itemDropInfo in itemDropInfos) {
				if (itemDropInfo.required) {
					continue;
				}
				currentWeight += itemDropInfo.dropWeight;
				if (currentWeight >= randomWeight) {
					return itemDropInfo;
				}
			}
			
			return default;
		}

		public ItemDropInfo[] GetRequiredDropItems() {
			Dictionary<int, ItemDropCollection> itemDropCollections = itemDropCollectionsProperty.RealValues.Value;
			
			if(itemDropCollections == null || itemDropCollections.Count == 0) {
				return default;
			}


			ItemDropCollection itemDropCollection = GetItemDropCollection();
			
			ItemDropInfo[] itemDropInfos = itemDropCollection.ItemDropInfos;
			if (itemDropInfos == null || itemDropInfos.Length == 0) {
				return default;
			}
			
			return itemDropInfos.Where(info => info.required).ToArray();
		}

		public ItemDropCollection GetItemDropCollection() {
			Dictionary<int, ItemDropCollection> itemDropCollections = itemDropCollectionsProperty.RealValues.Value;
			
			if(itemDropCollections == null || itemDropCollections.Count == 0) {
				return default;
			}
			
			int rarity = GetRarity();

			int targetRarity = rarity;
			if (!itemDropCollections.ContainsKey(targetRarity)) {
				int minRarity = itemDropCollections.Keys.Min();
				int maxRarity = itemDropCollections.Keys.Max();
				if (targetRarity < minRarity) {
					targetRarity = minRarity;
				}
				else if (targetRarity > maxRarity) {
					targetRarity = maxRarity;
				}
				else {
					targetRarity = itemDropCollections.Keys.Where(r => r <= targetRarity).Max();
				}
			}
			
			ItemDropCollection itemDropCollection = itemDropCollections[targetRarity];
			return itemDropCollection;
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			RegisterInitialProperty<IItemDropCollectionsProperty>(new ItemDropCollections());
		}
	}
}