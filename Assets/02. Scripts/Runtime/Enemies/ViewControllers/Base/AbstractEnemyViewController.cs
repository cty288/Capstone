using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Rewards;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using Runtime.Temporary;
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.Collision;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;
using Random = UnityEngine.Random;

namespace Runtime.Enemies.ViewControllers.Base {
	[RequireComponent(typeof(AnimationSMBManager))]
	public abstract class AbstractEnemyViewController<T> : AbstractCreatureViewController<T>, IEnemyViewController, IHitResponder
		where T : class, IEnemyEntity, new() {
		IEnemyEntity IEnemyViewController.EnemyEntity => BoundEntity;
		
		public int Danger {  get; }
	
		public int MaxHealth { get; }
	
		//[Bind(PropertyName.health, nameof(GetCurrentHealth), nameof(OnCurrentHealthChanged))]
		public int CurrentHealth { get; }
		

		protected IEnemyEntityModel enemyModel;
		
		protected HealthBar currentHealthBar = null;

		
		protected List<GameObject> hitObjects = new List<GameObject>();
		
		protected ILevelModel levelModel;
		protected AnimationSMBManager animationSMBManager;
		private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
		private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;
		protected BehaviorTree _behaviorTree;

		[Header("(Temporary) Weapon Parts Drops")] [SerializeField]
		private float weaponPartsDropChance = 0.1f;
		[SerializeField]
		private Vector2Int weaponPartsDropCountRange = new Vector2Int(1, 1);
		
		
		protected override void Awake() {
			base.Awake();
			
			enemyModel = this.GetModel<IEnemyEntityModel>();
			animationSMBManager = GetComponent<AnimationSMBManager>();
			animationSMBManager.Event.AddListener(OnAnimationEvent);
			levelModel = this.GetModel<ILevelModel>();
			_behaviorTree = GetComponent<BehaviorTree>();
		}

		protected abstract void OnAnimationEvent(string eventName);

		protected abstract HealthBar OnSpawnHealthBar();

		protected abstract void OnDestroyHealthBar(HealthBar healthBar);

		protected override void OnStart() {
			base.OnStart();
			currentHealthBar = OnSpawnHealthBar();
			if (currentHealthBar != null) {
				currentHealthBar.SetEntity(BoundEntity.HealthProperty.RealValue, BoundEntity);
			}
			
		}

		public void EnableBehaviorTree(bool enable)
		{
			_behaviorTree.enabled = enable;
		}

		protected override void OnBindEntityProperty() {
			Bind("Danger", BoundEntity.GetDanger());
			Bind<HealthInfo, int>("MaxHealth", BoundEntity.GetHealth(), info => info.MaxHealth);
			Bind<HealthInfo, int>("CurrentHealth", BoundEntity.GetHealth(), info => info.CurrentHealth);
		}
		
		public override ICreature OnInitEntity(int level, int rarity){
			if (enemyModel == null) {
				enemyModel = this.GetModel<IEnemyEntityModel>();
			}

			EnemyBuilder<T> builder = enemyModel.GetEnemyBuilder<T>(rarity);
			builder.SetProperty(new PropertyNameInfo(PropertyName.level_number), level);

			return OnInitEnemyEntity(builder);
		}
		

		protected abstract IEnemyEntity OnInitEnemyEntity(EnemyBuilder<T> builder);

		protected dynamic GetMaxHealth(dynamic info) {
			return info.MaxHealth;
		}
	
		protected dynamic GetCurrentHealth(dynamic info) {
			return info.CurrentHealth;
		}

		

		protected void OnCurrentHealthChanged(int oldValue, int newValue) {
			// Debug.Log("CurrentHealth changed from " + oldValue + " to " + newValue);
		}


		protected override void OnEntityDie(ICanDealDamage damagedealer) {
			base.OnEntityDie(damagedealer);

			SpawnWeaponParts(damagedealer);
		}

		protected async UniTask SpawnWeaponParts(ICanDealDamage damagedealer) {
			if (damagedealer == null || !damagedealer.GetRootDamageDealer().IsSameFaction(BoundEntity)) {
				bool spawnWeaponParts = UnityEngine.Random.value < weaponPartsDropChance;

				if (!spawnWeaponParts) {
					return;
				}
				int minLevel = Mathf.Max(levelModel.CurrentLevelCount.Value - 1, 1);
				int maxLevel = Mathf.Min(levelModel.CurrentLevelCount.Value, LevelModel.MAX_LEVEL);
				
				RewardBatch batch = default;
				if (minLevel != maxLevel) {
					batch = new RewardBatch(RewardType.Random_WeaponParts,
						new Dictionary<int, int>() {
							{minLevel, 8},
							{maxLevel, 2}
						}, weaponPartsDropCountRange);

				}
				else {
					batch = new RewardBatch(RewardType.Random_WeaponParts,
						new Dictionary<int, int>() {
							{minLevel, weaponPartsDropCountRange.x}
						}, weaponPartsDropCountRange);
				}

				
				


				List<GameObject> spawnedResources = await RewardDisperser.Singleton.DisperseRewards(
					new List<RewardBatch>() {batch}, null, CurrencyType.Time);


				Transform spawnTr = gameObject.transform;
				Vector3 spawnPos = spawnTr.position;
				
				if(Physics.Raycast(spawnTr.position, spawnTr.forward, out RaycastHit hit, 10,
					   LayerMask.GetMask("Ground", "Default", "Wall"))) {

					spawnPos = hit.point;
				}

				foreach (var resource in spawnedResources) {
					resource.transform.position = spawnPos;
					resource.transform.rotation = Quaternion.identity;
					Rigidbody rb = resource.GetComponent<Rigidbody>();
					if (rb) {
						//45 degree upword, random direction
						//rb.AddForce(Quaternion.Euler(45, Random.Range(0, 360), 0) * Vector3.up * 5, ForceMode.Impulse);
					}
				}
			}
			
			

		}


		protected override void OnReadyToRecycle() {
			base.OnReadyToRecycle();
			if (currentHealthBar) {
				currentHealthBar.DestroyHealthBar();
				OnDestroyHealthBar(currentHealthBar);
			}
			currentHealthBar = null;
			OnModifyDamageCountCallbackList.Clear();
			_onDealDamageCallback = null;
			_onKillDamageableCallback = null;
		}

		protected override int GetSpawnedCombatCurrencyAmount() {
			int currentLevel = levelModel.CurrentLevelCount;
			float referenceCount = BoundEntity.GetRealSpawnCost(currentLevel, BoundEntity.GetRarity());
			//+- 10%
			float randomCount = UnityEngine.Random.Range(-referenceCount * 0.1f, referenceCount * 0.1f);
			int result = Mathf.RoundToInt(referenceCount + randomCount);
			result = Mathf.Clamp(result, 1, int.MaxValue);
			return result;
		}

		public virtual bool CheckHit(HitData data) {
			if (data.Hurtbox.Owner == gameObject) { return false; }
			else if (hitObjects.Contains(data.Hurtbox.Owner)) { return false; }
			else { return true; }
		}

		public virtual void HitResponse(HitData data) {
			hitObjects.Add(data.Hurtbox?.Owner);
		}

		public HitData OnModifyHitData(HitData data) {
			return data;
		}
		
		public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
			//BoundEntity?.OnKillDamageable(damageable);
		}

		public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
			//BoundEntity?.OnDealDamage(damageable, damage);
		}

		public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

		Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
			get => _onDealDamageCallback;
			set => _onDealDamageCallback = value;
		}

		Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
			get => _onKillDamageableCallback;
			set => _onKillDamageableCallback = value;
		}
		public ICanDealDamage ParentDamageDealer => BoundEntity;
		public Transform GetTransform() {
			return transform;
		}

		/*public ICanDealDamageRootEntity RootDamageDealer => BoundEntity?.RootDamageDealer;
		
		
		
		public ICanDealDamageRootViewController RootViewController => this;*/
		
	}
}
