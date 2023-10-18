using System;
using BehaviorDesigner.Runtime;
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
	}

	[Serializable]
	public struct ItemDropCollection {
		public int Rarity;
		public ItemDropInfo[] ItemDropInfos;
	}
	
	[Serializable]
	public struct ItemDropInfo {
		public GameObject prefab;
		public Vector2Int dropCountRange;
		public float dropChance;
	}
	
	/// <summary>
	/// An abstract view controller for creature entity (like player, enemy, etc.)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TEntityModel"></typeparam>
	public abstract class AbstractCreatureViewController<T> : AbstractDamagableViewController<T>, ICreatureViewController
		where T : class, IHaveCustomProperties, IHaveTags, IDamageable, ICreature {
		[SerializeField] protected ItemDropCollection[] itemDropCollections;
		
			
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
	}
}