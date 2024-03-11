using System;
using System.Collections.Generic;
using System.Threading;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.VFX;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.GameResources;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Runtime.DataFramework.ViewControllers.Entities {

	public interface ICreatureViewController : IDamageableViewController {
		public BoxCollider SpawnSizeCollider { get; }
		
		public ICreature OnInitEntity(int level, int rarity);
		CancellationToken GetCancellationTokenOnStunnedOrDie();
	}

	[Serializable]
	public struct ItemDropCollection {
		//public int Rarity;
		public Vector2Int TotalDropCountRange;
		//public Vector2Int CombatCurrencyDropCountRange;
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
	public abstract class AbstractCreatureViewController<T> : AbstractDamagableViewController<T>, ICreatureViewController, IBuffableVFXViewController
		where T : class, IHaveCustomProperties, IHaveTags, IDamageable, ICreature {
		//[SerializeField] protected List<ItemDropCollection> baseItemDropCollections;

		private static int combatCurrencyAmountPerItem = 5;
		[Header("Rarity Base Value")]
		[SerializeField] protected int rarityBaseValueBuiltFromInspector = 1;
		protected NavMeshAgent navMeshAgent;
		protected BehaviorTree behaviorTree;
		
		[Header("Nav Mesh")]
		[SerializeField] protected bool randomizeNavMeshPriority = true;
		
		[Header("Creature Recycle Settings")]
		[SerializeField]
		private bool autoRemoveEntityWhenDie = true;
		
		[SerializeField]public Transform[] VFXFramer { get; }

		private CancellationTokenSource ctsWhenDieOrStunned
			= new CancellationTokenSource();
		
		private Rigidbody rb;
		protected override void Awake() {
			base.Awake();
			navMeshAgent = GetComponent<NavMeshAgent>();
			behaviorTree = GetComponent<BehaviorTree>();
			if (navMeshAgent) {
				navMeshAgent.enabled = false;
				if (randomizeNavMeshPriority) {
					navMeshAgent.avoidancePriority = Random.Range(0, 100);
				}
			}
			
			if (behaviorTree) {
				behaviorTree.enabled = false;
			}
			
			rb = GetComponent<Rigidbody>();
		}
		
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;

		protected override void OnStart() {
			base.OnStart();
			if (navMeshAgent) {
				navMeshAgent.enabled = true;
			}
			
			if (behaviorTree) {
				print($"{BoundEntity.EntityName} behavior tree enabled");
				behaviorTree.enabled = true;
				behaviorTree.Start();
			}

			BoundEntity.StunnedCounter.Count.RegisterWithInitValue(OnStunnedCounterChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);

			//ctsWhenDieOrStunned = new CancellationTokenSource();
		}

		private void OnStunnedCounterChanged(int oldCount, int newCount) {
			if (newCount <= 0) {
				ctsWhenDieOrStunned.Cancel();
				ctsWhenDieOrStunned = new CancellationTokenSource();
			}
			OnStunned(newCount > 0);
		}

		protected virtual void OnStunned(bool isStunned) {
			if (behaviorTree) {
				behaviorTree.enabled = !isStunned;
			}
			
			
			if (navMeshAgent) {
				if (isStunned) {
					navMeshAgent.velocity = Vector3.zero;
				}
				navMeshAgent.enabled = !isStunned;
			}
			
			if(rb && isStunned) {
				rb.velocity = Vector3.zero;
			}

			Animator animator = GetComponentInChildren<Animator>();
			if (animator) {
				animator.enabled = !isStunned;
			}
		}

		protected abstract MikroAction WaitingForDeathCondition();
		protected virtual void OnDieWaitEnd(ICanDealDamage damagedealer) {
			SpawnDeathDroppedItemsAndCurrency(damagedealer);
			if (autoRemoveEntityWhenDie) {
				GlobalEntities.GetEntityAndModel(BoundEntity.UUID).Item2.RemoveEntity(BoundEntity.UUID);
			}
		
		}

		protected override void Update() {
			base.Update();
			if (Input.GetKeyDown(KeyCode.O)) {
				BoundEntity.StunnedCounter.Retain();
			}
			
			if (Input.GetKeyDown(KeyCode.I)) {
				BoundEntity.StunnedCounter.Release();
			}
		}

		private void SpawnDeathDroppedItemsAndCurrency(ICanDealDamage damagedealer) {
			if (damagedealer == null || damagedealer.GetRootDamageDealer() == null ||
			    damagedealer.GetRootDamageDealer().IsSameFaction(this)) {
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

			int attempt = 0;
			while (dropCount < totalDropCount) {
				ItemDropInfo info = BoundEntity.GetRandomDropItem();
				
				if (String.IsNullOrEmpty(info.prefabName)) {
					throw new Exception("ItemDropInfo prefabName is null or empty in " + BoundEntity.GetType().Name +
					                    " entity.");
				}
				dropCount += GenerateDropItem(info, totalDropCount);
				attempt++;
				if (attempt >= 100000) {
					Debug.Log("WHILE LOOP CRASHED!");
					break;
				}
			}

			if (damagedealer.GetRootDamageDealer().CurrentFaction.Value == Faction.Friendly) { //killed by the player or friendly
				int combatCurrencyDropCount = GetSpawnedCombatCurrencyAmount();
				if (combatCurrencyDropCount > 0) {
					GeneratePickableCombatCurrency(combatCurrencyDropCount);
				}
			}

		}

		protected override void OnEntityDie(ICanDealDamage damagedealer) {
			MikroAction action = WaitingForDeathCondition();
			if (action != null) {
				action.OnEndedCallback += () => {
					OnDieWaitEnd(damagedealer);
				};
				action.Execute();
			}
			else {
				OnEntityDieWithoutCallback(damagedealer);
			}
			ctsWhenDieOrStunned.Cancel();
			ctsWhenDieOrStunned = new CancellationTokenSource();
		}

		private async UniTask OnEntityDieWithoutCallback(ICanDealDamage damagedealer) {
			await UniTask.Yield();
			OnDieWaitEnd(damagedealer);
		}
		
		protected abstract int GetSpawnedCombatCurrencyAmount();

		protected void GeneratePickableCombatCurrency(int totalCount) {
			//drop num = totalCount / combatCurrencyAmountPerItem
			//if mod != 0, drop num += 1, and drop mod
			int dropCount = totalCount / combatCurrencyAmountPerItem;
			
			Vector3 spawnBasePosition = transform.position;
			spawnBasePosition.y = SpawnSizeCollider.bounds.max.y;
			
			Vector3 FindSpawnPosition() {
				Vector3 spawnPosition = spawnBasePosition;
				spawnPosition.x += Random.Range(-SpawnSizeCollider.bounds.extents.x/2, SpawnSizeCollider.bounds.extents.x/2);
				spawnPosition.z += Random.Range(-SpawnSizeCollider.bounds.extents.z/2, SpawnSizeCollider.bounds.extents.z/2);
				return spawnPosition;
			}
			
			for (int i = 0; i < dropCount; i++) {
				GameObject spawnedResource =
					ResourceVCFactory.Singleton.SpawnPickableCurrency(CurrencyType.Combat, combatCurrencyAmountPerItem);
				
				Vector3 spawnPosition = FindSpawnPosition();
				spawnedResource.transform.position = spawnPosition;
				totalCount -= combatCurrencyAmountPerItem;
			}
			
			if (totalCount > 0) {
				GameObject spawnedResource =
					ResourceVCFactory.Singleton.SpawnPickableCurrency(CurrencyType.Combat, totalCount);
				
				Vector3 spawnPosition = FindSpawnPosition();
				spawnedResource.transform.position = spawnPosition;
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
		public BoxCollider SpawnSizeCollider { get; protected set; }

		public abstract ICreature OnInitEntity(int level, int rarity);
		public CancellationToken GetCancellationTokenOnStunnedOrDie() {
			return ctsWhenDieOrStunned.Token;
		}
	}
}