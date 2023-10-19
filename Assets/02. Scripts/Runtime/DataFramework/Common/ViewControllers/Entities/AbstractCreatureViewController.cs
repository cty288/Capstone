using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using BehaviorDesigner.Runtime;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.DataFramework.ViewControllers.Entities {

	public interface ICreatureViewController : IDamageableViewController {
		public BoxCollider SpawnSizeCollider { get; }
		
		public ICreature OnInitEntity(int level, int rarity);
	}

	[Serializable]
	public struct ItemDropCollection {
		public int Rarity;
		public ItemDropInfo[] ItemDropInfos;
	}
	
	[Serializable]
	public struct ItemDropInfo {
		public string prefabName;
		public Vector2Int dropCountRange;
		public float dropChance;
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
		[SerializeField] protected List<ItemDropCollection> baseItemDropCollections;
		
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

		protected override IEntity OnBuildNewEntity() {
			int level = this.GetModel<ILevelModel>().CurrentLevelCount.Value;
			return OnInitEntity(level, rarityBaseValueBuiltFromInspector, ProcessItemDropCollections(baseItemDropCollections));
		}
		
		private Dictionary<int, ItemDropCollection> ProcessItemDropCollections(List<ItemDropCollection> itemDropCollections) {
			Dictionary<int, ItemDropCollection> result = new Dictionary<int, ItemDropCollection>();
			itemDropCollections.Sort((x, y) => x.Rarity.CompareTo(y.Rarity)); // sort in ascending order
			
			foreach (var itemDropCollection in itemDropCollections) {
				result.TryAdd(itemDropCollection.Rarity, itemDropCollection);
			}

			return result;
		}

		protected abstract ICreature OnInitEntity(int level, int rarity, Dictionary<int,ItemDropCollection> itemDropCollections);

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

		public ICreature OnInitEntity(int level, int rarity) {
			return OnInitEntity(level, rarity, ProcessItemDropCollections(baseItemDropCollections));
		}
	}
}