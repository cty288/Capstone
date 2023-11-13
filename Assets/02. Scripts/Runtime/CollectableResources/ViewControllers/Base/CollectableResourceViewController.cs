﻿using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.Model;
using _02._Scripts.Runtime.CollectableResources.Model.Properties;
using _02._Scripts.Runtime.Currency.Model;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model.Properties;
using Runtime.GameResources;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;
using Random = UnityEngine.Random;

namespace _02._Scripts.Runtime.CollectableResources.ViewControllers.Base {
	public interface ICollectableResourceViewController : IEntityViewController {
		public IEntity OnBuildNewEntity(int level);
		
		public BoxCollider SpawnSizeCollider { get; }
	}
	public abstract class CollectableResourceViewController<T> : AbstractBasicEntityViewController<T>
		, ICollectableResourceViewController where T : class, IHaveCustomProperties, IHaveTags, ICollectableEntity, new() {
		protected override bool CanAutoRemoveEntityWhenLevelEnd => true;
		protected ICommonEntityModel commonEntityModel;

		//[SerializeField] private List<CollectableResourceCurrencyInfo> collectableResourceCurrencyInfos;
		//[SerializeField] private int levelBuildFromInspector = 1;
		[SerializeField] private string overriddenName;
		[SerializeField] private int currencyAmountPerItem = 2;
		protected override void Awake() {
			base.Awake();
			commonEntityModel = this.GetModel<ICommonEntityModel>();
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
			return builder.Build();
		}

		protected override void OnPlayerPressInteract() {
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
			
		}

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
	}
}