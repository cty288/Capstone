using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Mineral;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat {

	public class OnCombatBuffGenerateExplostion : WeaponBuildBuffEvent {
		public int Damage { get; set; }
		public GameObject HurtboxOwner { get; set; }
		public Vector3 HitPoint { get; set; }
		public ICanDealDamage Attacker { get; set; }
		
		public CombatBuff Buff { get; set; }
	}
	
	public class OnCombatBuffExplosionDealDamge : WeaponBuildBuffEvent {
		public int Damage { get; set; }
		public IDamageable Target { get; set; }
		public CombatBuff Buff { get; set; }
		public bool IsDie { get; set; }
	}

	public class CombatBuffOnModifyBaseAmmoRecoverEvent : ModifyValueEvent<int> {
		public CombatBuffOnModifyBaseAmmoRecoverEvent(int value) : base(value) {
		}
	}

	public class OnCombatBuffModifyExplosionDOR : ModifyValueEvent<int> {
		public OnCombatBuffModifyExplosionDOR(int value) : base(value) {
		}
	}
	
	public class OnCombatBuffModifyExplosionChance : ModifyValueEvent<float> {
		public OnCombatBuffModifyExplosionChance(float value) : base(value) {
		}
	}
	
	

	public class CombatBuffInternalExplosion : ICanDealDamage {
		private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
		private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>();
		public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
			
		}

		public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
			 
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

		public ICanDealDamage ParentDamageDealer { get; }
		
		public CombatBuffInternalExplosion(ICanDealDamage parentDamageDealer) {
			ParentDamageDealer = parentDamageDealer;
			CurrentFaction.Value = parentDamageDealer.CurrentFaction.Value;
		}
	}
	
	
	public class CombatBuff : WeaponBuildBuff<CombatBuff>, ICanGetUtility {
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = -1;
		
		private ResLoader resLoader;
		
		public override void OnInitialize() {
			if (weaponEntity == null) {
				return;
			}

			// Initialize Pool
			bulletInVFXPool = GameObjectPoolManager.Singleton.CreatePoolFromAB("testGunVFXIn", null, 3, 10, out GameObject prefab0);
			bulletOutVFXPool = GameObjectPoolManager.Singleton.CreatePoolFromAB("testGunVFXOut", null, 3, 10, out GameObject prefab2);
			bulletHitVFXPool = GameObjectPoolManager.Singleton.CreatePoolFromAB("TestExplode", null, 3, 10, out GameObject prefab1);
			
			var vc = weaponEntity.GetBoundViewController();
			AllocateBuffVFX(vc as IWeaponVFX, vc as IHitScanWeaponVFX);
				
			weaponEntity.RegisterOnModifyHitData(OnWeaponModifyHitData);
			weaponEntity.RegisterOnKillDamageable(OnKillDamageable);
			weaponEntity.RegisterOnDealDamage(OnDealDamage);
			
			//weaponEntity.RegisterOnDealDamage();
			
			resLoader = this.GetUtility<ResLoader>();
			
		}

		private void OnDealDamage(ICanDealDamage source, IDamageable target, int damage) {
			
		}
		public override string[] GetAllLevelDescriptions() {
			int displayedChance = Mathf.RoundToInt(GetBuffPropertyAtLevel<float>("chance", 1) * 100);
			int totalDamage =
				GetBuffPropertyAtLevel<int>("damage_per_rarity", 1) * weaponEntity.GetRarity();

			int displayedHealthThreshold = Mathf.RoundToInt(GetBuffPropertyAtLevel<float>("health_kill", 2) * 100);

			return new[] {
				Localization.GetFormat("BUILD_BUFF_COMBAT_1", displayedChance, totalDamage),
				Localization.GetFormat("BUILD_BUFF_COMBAT_2", displayedHealthThreshold),
				Localization.GetFormat("BUILD_BUFF_COMBAT_3", GetBuffPropertyAtLevel<int>("ammo_recovery_base", 3),
					GetBuffPropertyAtLevel<int>("ammo_recovery_kill", 3))
			};
		}

		private void OnKillDamageable(ICanDealDamage source, IDamageable target) {
			if (Level < 3) {
				return;
			}
			
			if (source is CombatBuffInternalExplosion) {
				int ammoRecovered = GetBuffPropertyAtCurrentLevel<int>("ammo_recovery_kill");
				weaponEntity.AddAmmo(ammoRecovered);
			}
		}

		public void GenerateExplosion(int damage, GameObject hurtboxOwner, Vector3 hitPoint, ICanDealDamage attacker) {
			float range = GetBuffPropertyAtCurrentLevel<float>("range");
			
			GameObject explosionGo = resLoader.LoadSync<GameObject>("Explosion_CombatBuff");
			CombatBuffExplosion explosion =
				GameObject.Instantiate(explosionGo, hitPoint, Quaternion.identity)
					.GetComponent<CombatBuffExplosion>();
			
			explosion.Init(weaponEntity.CurrentFaction.Value, 0, range, null, weaponEntity);
			
			if (hurtboxOwner && hurtboxOwner.TryGetComponent<IDamageableViewController>(out IDamageableViewController damageableViewController)) {
				bool killTriggered = false;
				if (Level >= 2) {
					if (damageableViewController.DamageableEntity is INormalEnemyEntity enemyEntity) {
						int maxHealth = enemyEntity.GetMaxHealth();
						int currentHealth = enemyEntity.GetCurrentHealth();
						float healthPercentage = (float)currentHealth / maxHealth;
						float killThreshold = GetBuffPropertyAtCurrentLevel<float>("health_kill");

						if (healthPercentage <= killThreshold) {
							enemyEntity.Kill(new CombatBuffInternalExplosion(attacker));
							killTriggered = true;
						}
					}
				}

				bool isDie = false;
				if (!killTriggered) {
					damageableViewController.DamageableEntity.TakeDamage(damage,
						new CombatBuffInternalExplosion(attacker), out isDie);
				}
				
				SendWeaponBuildBuffEvent<OnCombatBuffExplosionDealDamge>(new OnCombatBuffExplosionDealDamge() {
					Buff = this,
					Damage = damage,
					IsDie = killTriggered || isDie,
					Target = damageableViewController.DamageableEntity
				});
			}


			int ammoRecovered = 0;
			if (Level >= 3) {
				ammoRecovered += GetBuffPropertyAtCurrentLevel<int>("ammo_recovery_base");
			}
			ammoRecovered = weaponEntity
					.SendModifyValueEvent(new CombatBuffOnModifyBaseAmmoRecoverEvent(ammoRecovered)).Value;

			if (ammoRecovered > 0) {
				weaponEntity.AddAmmo(ammoRecovered);
			}
		}

		private HitData OnWeaponModifyHitData(HitData hit, IWeaponEntity weapon)
		{

			var hitscan = (hit.HitDetector as HitScan);
			if (hitscan != null)
			{
				var vc = weaponEntity.GetBoundViewController();
				AllocateBuffVFX(vc as IWeaponVFX, vc as IHitScanWeaponVFX);
			}
			
			float chance = GetBuffPropertyAtCurrentLevel<float>("chance");
			chance = weaponEntity.SendModifyValueEvent(new OnCombatBuffModifyExplosionChance(chance)).Value;
			
			if (Random.Range(0f, 1f) <= chance) {
				int explosionDamagePerRarity = GetBuffPropertyAtCurrentLevel<int>("damage_per_rarity");
				explosionDamagePerRarity = weaponEntity
					.SendModifyValueEvent(new OnCombatBuffModifyExplosionDOR(explosionDamagePerRarity)).Value;
				
				
				int damage = Mathf.RoundToInt(explosionDamagePerRarity * weaponEntity.GetRarity());
				GameObject owner = hit.Hurtbox?.Owner;


				GenerateExplosion(damage, owner, hit.HitPoint, hit.Attacker);

				SendWeaponBuildBuffEvent<OnCombatBuffGenerateExplostion>
				(new OnCombatBuffGenerateExplostion() {
					Attacker = hit.Attacker,
					Damage = damage,
					HitPoint = hit.HitPoint,
					HurtboxOwner = owner,
					Buff = this
				});

			}

			return hit;
		}

		public override void OnStart()
		{
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		public override void OnRecycled() {
			if (weaponEntity != null)
			{
				DeallocateBuffVFX();
				weaponEntity.UnRegisterOnModifyHitData(OnWeaponModifyHitData);
				weaponEntity.UnregisterOnKillDamageable(OnKillDamageable);
				weaponEntity.UnregisterOnDealDamage(OnDealDamage);
			}
			base.OnRecycled();
		}
		

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		protected override bool OnValidate() {
			return true;
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}