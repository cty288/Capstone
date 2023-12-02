using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.Model;
using _02._Scripts.Runtime.CollectableResources.Model.Properties;
using _02._Scripts.Runtime.Currency.Model;
using AYellowpaper.SerializedCollections;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model.Properties;
using Runtime.GameResources;
using Runtime.Utilities.Collision;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;
using Random = UnityEngine.Random;

namespace _02._Scripts.Runtime.CollectableResources.ViewControllers.Base {
	public interface ICollectableResourceViewController : IEntityViewController {
		public IEntity OnBuildNewEntity(int level);
		
		public BoxCollider SpawnSizeCollider { get; }
	}
	public abstract class CollectableResourceViewController<T> : AbstractBasicEntityViewController<T>
		, ICollectableResourceViewController, IHurtResponder where T : class, IHaveCustomProperties, IHaveTags, ICollectableEntity, new() {
		protected override bool CanAutoRemoveEntityWhenLevelEnd => true;
		protected ICommonEntityModel commonEntityModel;

		//[SerializeField] private List<CollectableResourceCurrencyInfo> collectableResourceCurrencyInfos;
		//[SerializeField] private int levelBuildFromInspector = 1;
		[SerializeField] private string overriddenName;
		[SerializeField] private int currencyAmountPerItem = 2;

		[SerializeField] private int totalShootTime = 3;
		[SerializeField] private int damageRequiredPerShoot = 10;

		[SerializeField]
		[SerializedDictionary("Rarity", "Item Drop Collections")]
		private SerializedDictionary<int, ItemDropCollection> overriddenItemDropCollections;
		
		
		
		
		public List<CollectableResourceCurrencyInfo> overriddenCollectableResourceCurrencyInfos;

		private int accumulatedDamage = 0;
		private int realTotalShootTime;
		private int totalSpawnAmount = 0;
		protected override void Awake() {
			base.Awake();
			commonEntityModel = this.GetModel<ICommonEntityModel>();
			GetComponentInChildren<IHurtbox>(true).HurtResponder = this;
		}

		protected override IEntity OnBuildNewEntity() {
			return OnBuildNewEntity(1);
		}


		public IEntity OnBuildNewEntity(int level) {
			var builder = commonEntityModel.GetBuilder<T>(level);
				//.SetProperty(new PropertyNameInfo(PropertyName.level_number), level);

			if (!string.IsNullOrEmpty(overriddenName)) {
				builder.OverrideName(overriddenName);
			}

			builder.SetProperty(new PropertyNameInfo(PropertyName.level_number), level).FromConfig();
			if (overriddenItemDropCollections != null && overriddenItemDropCollections.Count > 0) {
				builder.SetProperty<Dictionary<int, ItemDropCollection>>
					(new PropertyNameInfo(PropertyName.item_drop_collections), overriddenItemDropCollections);
			}
			
			if (overriddenCollectableResourceCurrencyInfos != null && overriddenCollectableResourceCurrencyInfos.Count > 0) {
				builder.SetProperty(new PropertyNameInfo(PropertyName.collectable_resource_currency_list),
					overriddenCollectableResourceCurrencyInfos);
			}
			
			return builder.Build();
		}

		protected override void OnEntityStart() {
			var currencyList = BoundEntity.GetCollectableResourceCurrencyList().RealValue.Value;
			int level = BoundEntity.GetProperty<ILevelNumberProperty>().RealValue.Value;

			CollectableResourceCurrencyInfo currencyInfo = currencyList[0];
			if (level > currencyList.Count) {
				currencyInfo = currencyList[currencyList.Count - 1];
			}
			else {
				currencyInfo = currencyList[level - 1];
			}

			totalSpawnAmount = Random.Range(currencyInfo.amountRange.x, currencyInfo.amountRange.y + 1);
			
			realTotalShootTime = Mathf.Min(totalShootTime, totalSpawnAmount);
			
			
		}

		/*protected override void OnPlayerPressInteract() {
			base.OnPlayerPressInteract();
			var currencyList = BoundEntity.GetCollectableResourceCurrencyList().RealValue.Value;
			int level = BoundEntity.GetProperty<ILevelNumberProperty>().RealValue.Value;

			CollectableResourceCurrencyInfo currencyInfo = currencyList[0];
			if (level > currencyList.Count) {
				currencyInfo = currencyList[currencyList.Count - 1];
			}
			else {
				currencyInfo = currencyList[level - 1];
			}
			GeneratePickableCombatCurrency(currencyInfo.currencyType,
				Random.Range(currencyInfo.amountRange.x, currencyInfo.amountRange.y + 1));

			GenerateResources();
			
			commonEntityModel.RemoveEntity(BoundEntity.UUID);
			
		}*/

		private void GenerateResources() {
			ItemDropCollection itemDropCollection = BoundEntity.GetItemDropCollection();
			
			if (itemDropCollection.ItemDropInfos == null) {
				return;
			}
			
			
			ItemDropInfo[] requiredDrops = BoundEntity.GetRequiredDropItems();
			if (requiredDrops != null) {
				foreach (ItemDropInfo requiredDrop in requiredDrops) {
					GenerateDropItem(requiredDrop, Int32.MaxValue);
				}
			}
			
			int totalDropCount = Random.Range(itemDropCollection.TotalDropCountRange.x,
				itemDropCollection.TotalDropCountRange.y + 1);
			int dropCount = 0;

			while (dropCount < totalDropCount) {
				ItemDropInfo info = BoundEntity.GetRandomDropItem();
				
				if (String.IsNullOrEmpty(info.prefabName)) {
					throw new Exception("ItemDropInfo prefabName is null or empty in " + BoundEntity.GetType().Name +
					                    " entity.");
				}
				dropCount += GenerateDropItem(info, totalDropCount);

			}
		}
		
		protected int GenerateDropItem(ItemDropInfo info, int totalDropCount) {
			int dropCount = Random.Range(info.batchCountRange.x, info.batchCountRange.y + 1);
			Vector3 spawnBasePosition = transform.position;
			spawnBasePosition.y = SpawnSizeCollider.bounds.max.y;

			dropCount = Mathf.Min(dropCount, totalDropCount);
			for (int i = 0; i < dropCount; i++) {
				
				int rarity = Random.Range(info.rarityRange.x, info.rarityRange.y + 1);
				GameObject spawnedResource =
					ResourceVCFactory.Singleton.SpawnNewPickableResourceVC(info.prefabName, true, info.setRarity,
						rarity);
				
				Vector3 spawnPosition = spawnBasePosition;
				spawnPosition.x += Random.Range(-SpawnSizeCollider.bounds.extents.x/2, SpawnSizeCollider.bounds.extents.x/2);
				spawnPosition.z += Random.Range(-SpawnSizeCollider.bounds.extents.z/2, SpawnSizeCollider.bounds.extents.z/2);
				spawnedResource.transform.position = spawnPosition;
				
				if(spawnedResource.TryGetComponent<Rigidbody>(out var rigidbody)) {
					rigidbody.AddForce(Vector3.up * 3, ForceMode.Impulse);
				}
			}
			
			return dropCount;
			
		}

		protected void GeneratePickableCombatCurrency(CurrencyType type, int totalCount) {
			Vector3 spawnBasePosition = transform.position;
			spawnBasePosition.y = SpawnSizeCollider.bounds.max.y;
			
			Vector3 FindSpawnPosition() {
				Vector3 spawnPosition = spawnBasePosition;
				spawnPosition.x += Random.Range(-SpawnSizeCollider.bounds.extents.x/2, SpawnSizeCollider.bounds.extents.x/2);
				spawnPosition.z += Random.Range(-SpawnSizeCollider.bounds.extents.z/2, SpawnSizeCollider.bounds.extents.z/2);
				return spawnPosition;
			}


			int dropCount = totalCount / currencyAmountPerItem;
			for (int i = 0; i < dropCount; i++) {
				GameObject spawnedResource =
					ResourceVCFactory.Singleton.SpawnPickableCurrency(type, currencyAmountPerItem);
				
				Vector3 spawnPosition = FindSpawnPosition();
				spawnedResource.transform.position = spawnPosition;
				Vector3 forceDirection =
					(Vector3.up + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f))).normalized;
				spawnedResource.GetComponent<Rigidbody>().AddForce(forceDirection * 3, ForceMode.Impulse);
				totalCount -= currencyAmountPerItem;
			}
			
			if (totalCount > 0) {
				GameObject spawnedResource =
					ResourceVCFactory.Singleton.SpawnPickableCurrency(type, totalCount);
				
				Vector3 spawnPosition = FindSpawnPosition();
				spawnedResource.transform.position = spawnPosition;
			}
		}

		[field: SerializeField]
		public BoxCollider SpawnSizeCollider { get; protected set; }

		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Neutral);
		public bool CheckHurt(HitData data) {
			return data.Attacker.CurrentFaction.Value == Faction.Friendly;
		}

		public void HurtResponse(HitData data) {
			int damage = data.Damage;

			int spawnedTimeBefore = accumulatedDamage / damageRequiredPerShoot;
			accumulatedDamage += damage;
			accumulatedDamage = Mathf.Min(accumulatedDamage, realTotalShootTime * damageRequiredPerShoot);
			int spawnedTimeAfter = accumulatedDamage / damageRequiredPerShoot;
			
			//e.g. real total shoot time = 3, total spawn amount = 5 then get 1, 1, 3 resources each shoot
			int resourcePerSpawnTime = totalSpawnAmount / realTotalShootTime;
			int resourcesForFinalSpawnTime = totalSpawnAmount - resourcePerSpawnTime * (realTotalShootTime - 1);
			
			for (int i = spawnedTimeBefore; i < spawnedTimeAfter; i++) {
				if (i == realTotalShootTime - 1) {
					GeneratePickableCombatCurrency(CurrencyType.Combat, resourcesForFinalSpawnTime);
				}
				else {
					GeneratePickableCombatCurrency(CurrencyType.Combat, resourcePerSpawnTime);
				}
			}
			
			if(spawnedTimeAfter == realTotalShootTime) {
				GenerateResources();
				commonEntityModel.RemoveEntity(BoundEntity.UUID);
			}
			
		}

		public override void OnRecycled() {
			base.OnRecycled();
			accumulatedDamage = 0;
			realTotalShootTime = 0;
			totalSpawnAmount = 0;
		}
	}
}