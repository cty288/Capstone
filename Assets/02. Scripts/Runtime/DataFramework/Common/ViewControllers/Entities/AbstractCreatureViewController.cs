﻿using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using BehaviorDesigner.Runtime;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.GameResources;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Runtime.DataFramework.ViewControllers.Entities {

	public interface ICreatureViewController : IDamageableViewController {
		public BoxCollider SpawnSizeCollider { get; }
		
		public ICreature OnInitEntity(int level, int rarity);
	}

	[Serializable]
	public struct ItemDropCollection {
		//public int Rarity;
		public Vector2Int TotalDropCountRange;
		public ItemDropInfo[] ItemDropInfos;
	}
	
	[Serializable]
	public struct ItemDropInfo {
		public string prefabName;
		[FormerlySerializedAs("dropCountRange")] public Vector2Int batchCountRange;
		[FormerlySerializedAs("dropChance")] public float dropWeight;
		public bool required;
		public bool setRarity;
		public Vector2Int rarityRange;
	}
	
	/// <summary>
	/// An abstract view controller for creature entity (like player, enemy, etc.)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TEntityModel"></typeparam>
	public abstract class AbstractCreatureViewController<T> : AbstractDamagableViewController<T>, ICreatureViewController
		where T : class, IHaveCustomProperties, IHaveTags, IDamageable, ICreature {
		//[SerializeField] protected List<ItemDropCollection> baseItemDropCollections;
		
		[SerializeField] protected int rarityBaseValueBuiltFromInspector = 1;
		NavMeshAgent navMeshAgent;
		BehaviorTree behaviorTree;
		protected override void Awake() {
			base.Awake();
			navMeshAgent = GetComponent<NavMeshAgent>();
			behaviorTree = GetComponent<BehaviorTree>();
			if (navMeshAgent) {
				navMeshAgent.enabled = false;
			}
			
			if (behaviorTree) {
				behaviorTree.enabled = false;
			}
		}

		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;

		protected override void OnStart() {
			base.OnStart();
			if (navMeshAgent) {
				navMeshAgent.enabled = true;
			}
			
			if (behaviorTree) {
				behaviorTree.enabled = true;
				behaviorTree.Start();
			}

		}

		protected override void OnEntityDie(ICanDealDamage damagedealer) {
			if (damagedealer == null || damagedealer.RootDamageDealer == null ||
			    damagedealer.RootDamageDealer.IsSameFaction(this)) {
				return;
			}

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

		protected override IEntity OnBuildNewEntity() {
			int level = this.GetModel<ILevelModel>().CurrentLevelCount.Value;
			return OnInitEntity(level, rarityBaseValueBuiltFromInspector);
		}
		
		/*private Dictionary<int, ItemDropCollection> ProcessItemDropCollections(List<ItemDropCollection> itemDropCollections) {
			Dictionary<int, ItemDropCollection> result = new Dictionary<int, ItemDropCollection>();
			itemDropCollections.Sort((x, y) => x.Rarity.CompareTo(y.Rarity)); // sort in ascending order
			
			foreach (var itemDropCollection in itemDropCollections) {
				result.TryAdd(itemDropCollection.Rarity, itemDropCollection);
			}

			return result;
		}*/

		

		public override void OnRecycled() {
			base.OnRecycled();
			if (navMeshAgent) {
				navMeshAgent.enabled = false;
			}
			
			if (behaviorTree) {
				behaviorTree.enabled = false;
			}
			
		}

		[field: SerializeField]
		public BoxCollider SpawnSizeCollider { get; set; }

		public abstract ICreature OnInitEntity(int level, int rarity);
	}
}