using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Properties;
using Runtime.Player.Properties;
using Runtime.Player.ViewControllers;
using Runtime.Utilities.Collision;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.ViewControllers;
using UnityEngine;

namespace Runtime.Player {
	public interface IPlayerEntity : ICreature, IEntity, ICanDealDamage {
		IAccelerationForce GetAccelerationForce();
		IWalkSpeed GetWalkSpeed();
		ISprintSpeed GetSprintSpeed();
		ISlideSpeed GetSlideSpeed();
		IGroundDrag GetGroundDrag();
		IJumpForce GetJumpForce();
		IAdditionalGravity GetAdditionalGravity();
		IMaxSlideTime GetMaxSlideTime();
		ISlideForce GetSlideForce();
		IWallRunForce GetWallRunForce();

		IAirSpeedProperty GetAirSpeed();
		
		IArmorProperty GetMaxArmor();

		IHealthRecoverSpeed GetHealthRecoverSpeed();
		
		BindableProperty<float> Armor { get; }

		IArmorRecoverSpeedProperty GetArmorRecoverSpeed();
		
		IAirDrag GetAirDrag();
		MovementState GetMovementState();
		void SetMovementState(MovementState state);
		
		bool IsScopedIn();
		void SetScopedIn(bool state);
		
		void AddArmor(float amount);
		
		/// <summary>
		/// Unlike AddArmor, this will not trigger related events like RegisterOnModifyReceivedAddArmor
		/// </summary>
		/// <param name="amount"></param>
		void SetArmor(float amount);
		
		void ChangeMaxArmor(float amount);
		
		/// <summary>
		/// This is not dealing damage or healing, it's just changing the health value
		/// </summary>
		/// <param name="amount"></param>
		void ChangeHealth(int amount);

		public void RegisterOnModifyReceivedAddArmorAmount(Func<float, float> onModifyAddArmorAmount);
		
		public void UnRegisterOnModifyReceivedAddArmorAmount(Func<float, float> onModifyAddArmorAmount);
		//public void SetRootViewController(ICanDealDamageRootViewController rootViewController);
	}

	public struct OnPlayerKillEnemy {
		public int DamageDealt;
		public bool IsBoss;
		public IEnemyEntity Enemy;
	}
	
	public struct OnPlayerTakeDamage {
		public int DamageTaken;
		public float DamageToHealth;
		public HealthInfo HealthInfo;
		[CanBeNull]
		public HitData HitData;
	}
	
	public struct OnPlayerBuffUpdate {
		public IBuff Buff;
		public BuffUpdateEventType EventType;
	}

	public struct OnPlayerDie {
		public ICanDealDamage damageDealer;
		public HitData hitData;
	}
	public class PlayerEntity : AbstractCreature, IPlayerEntity, ICanDealDamage {
		public override string EntityName { get; set; } = "Player";
		
		private IAccelerationForce accelerationForce;
		
		private IWalkSpeed walkSpeed;
		private ISprintSpeed sprintSpeed;
		private ISlideSpeed slideSpeed;
		
		private IGroundDrag groundDrag;
		private IJumpForce jumpForce;
		private IAdditionalGravity additionalGravity;
		private IMaxSlideTime maxSlideTime;
		private ISlideForce slideForce;
		private IWallRunForce wallRunForce;
		private IAirDrag airDrag;
		private IAirSpeedProperty airSpeed;
		private IArmorProperty maxArmor;
		private IArmorRecoverSpeedProperty armorRecoverSpeed;
		private IHealthRecoverSpeed healthRecoverSpeed;
		public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

		

		Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
			get => _onDealDamageCallback;
			set => _onDealDamageCallback = value;
		}

		Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
			get => _onKillDamageableCallback;
			set => _onKillDamageableCallback = value;
		}

		private MovementState movementState;
		private bool scopedIn;
		private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
		private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;
		private HashSet<Func<float, float>> OnModifyAddArmorAmountCallbackList { get; } = new HashSet<Func<float, float>>();
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.PlayerEntityConfigTable;
		}

		public override void OnDoRecycle() {
			SafeObjectPool<PlayerEntity>.Singleton.Recycle(this as PlayerEntity);
		}

		public override void OnRecycle() {
			base.OnRecycle();
			OnModifyDamageCountCallbackList.Clear();
			_onDealDamageCallback = null;
			_onKillDamageableCallback = null;
			OnModifyAddArmorAmountCallbackList.Clear();
			
		}
		protected override void OnInitModifiers(int rarity) {
            
		}
		protected override string OnGetDescription(string defaultLocalizationKey) {
			return "";
		}
		

		protected override void OnEntityRegisterAdditionalProperties() {
			base.OnEntityRegisterAdditionalProperties();
			RegisterInitialProperty<IAccelerationForce>(new AccelerationForce());
			RegisterInitialProperty<IWalkSpeed>(new WalkSpeed());
			RegisterInitialProperty<ISprintSpeed>(new SprintSpeed());
			RegisterInitialProperty<ISlideSpeed>(new SlideSpeed());
			RegisterInitialProperty<IGroundDrag>(new GroundDrag());
			RegisterInitialProperty<IJumpForce>(new JumpForce());
			RegisterInitialProperty<IAdditionalGravity>(new AdditionalGravity());
			
			RegisterInitialProperty<IMaxSlideTime>(new MaxSlideTime());
			RegisterInitialProperty<ISlideForce>(new SlideForce());
			
			RegisterInitialProperty<IWallRunForce>(new WallRunForce());
			RegisterInitialProperty<IAirSpeedProperty>(new AirSpeed());
			RegisterInitialProperty<IAirDrag>(new AirDrag());
			
			RegisterInitialProperty<IArmorProperty>(new ArmorProperty());
			RegisterInitialProperty<IArmorRecoverSpeedProperty>(new ArmorRecoverSpeedProperty());

			RegisterInitialProperty<IHealthRecoverSpeed>(new HealthRecoverSpeed());
		}


		protected override void OnEntityStart(bool isLoadedFromSave) {
			if (!isLoadedFromSave) {
				Armor.Value =  maxArmor.RealValue.Value;
			}
		
		}

		public override void OnAwake() {
			base.OnAwake();
			accelerationForce = GetProperty<IAccelerationForce>();
			walkSpeed = GetProperty<IWalkSpeed>();
			sprintSpeed = GetProperty<ISprintSpeed>();
			slideSpeed = GetProperty<ISlideSpeed>();
			groundDrag = GetProperty<IGroundDrag>();
			jumpForce = GetProperty<IJumpForce>();
			additionalGravity = GetProperty<IAdditionalGravity>();
			maxSlideTime = GetProperty<IMaxSlideTime>();
			slideForce = GetProperty<ISlideForce>();
			wallRunForce = GetProperty<IWallRunForce>();
			airSpeed = GetProperty<IAirSpeedProperty>();
			airDrag = GetProperty<IAirDrag>();
			maxArmor = GetProperty<IArmorProperty>();
			armorRecoverSpeed = GetProperty<IArmorRecoverSpeedProperty>();
			healthRecoverSpeed = GetProperty<IHealthRecoverSpeed>();
		}

		public IAccelerationForce GetAccelerationForce() {
			return accelerationForce;

		}
		
		public IWalkSpeed GetWalkSpeed() {
			return walkSpeed;
		}
		
		public ISprintSpeed GetSprintSpeed() {
			return sprintSpeed;
		}
		
		public ISlideSpeed GetSlideSpeed() {
			return slideSpeed;
		}
		
		public IGroundDrag GetGroundDrag() {
			return groundDrag;
		}
		
		public IJumpForce GetJumpForce() {
			return jumpForce;
		}
		
		public IAdditionalGravity GetAdditionalGravity() {
			return additionalGravity;
		}
		
		public IMaxSlideTime GetMaxSlideTime() {
			return maxSlideTime;
		}
		
		public ISlideForce GetSlideForce() {
			return slideForce;
		}
		
		public IWallRunForce GetWallRunForce() {
			return wallRunForce;
		}

		public IAirSpeedProperty GetAirSpeed() {
			return airSpeed;
		}

		public IArmorProperty GetMaxArmor() {
			return maxArmor;
		}

		public IHealthRecoverSpeed GetHealthRecoverSpeed() {
			return healthRecoverSpeed;
		}

		[field: ES3Serializable]
		public BindableProperty<float> Armor { get; protected set; } = new BindableProperty<float>(0);

		public IArmorRecoverSpeedProperty GetArmorRecoverSpeed() {
			return armorRecoverSpeed;
		}

		public IAirDrag GetAirDrag() {
			return airDrag;
		}

		public MovementState GetMovementState()
		{
			return movementState;
		}

		public void SetMovementState(MovementState state)
		{
			movementState = state;
		}

		public bool IsScopedIn()
		{
			return scopedIn;
		}

		public void SetScopedIn(bool state)
		{
			scopedIn = state;
		}

		public void AddArmor(float amount) {
			foreach (var onModifyAddArmorAmount in OnModifyAddArmorAmountCallbackList) {
				amount = onModifyAddArmorAmount(amount);
			}
			
			float maxArmor = this.maxArmor.RealValue;
			
			if (Armor.Value < maxArmor) {
				float validAmount = Mathf.Min(maxArmor - Armor.Value, amount);
				this.Armor.Value +=  validAmount;
			}
		}

		public void SetArmor(float amount) {
			Armor.Value = Mathf.Clamp(amount, 0, maxArmor.RealValue.Value);
		}

		public void ChangeMaxArmor(float amount) {
			if (amount > 0) {
				maxArmor.RealValue.Value += amount;
				Armor.Value +=  amount;
			}
			else {
				maxArmor.RealValue.Value = Mathf.Max(0, maxArmor.RealValue.Value + amount);
				Armor.Value = Mathf.Max(0, Armor.Value + amount);
			}
		}

		public void ChangeHealth(int amount) {
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			HealthProperty.RealValue.Value = new HealthInfo(healthInfo.MaxHealth,
				Mathf.Clamp(healthInfo.CurrentHealth + amount, 0, healthInfo.MaxHealth));
			if (amount < 0) {
				Kill(null);
			}
		}


		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		protected override Faction GetDefaultFaction() {
			return Faction.Friendly;
		}

		public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
			
		}

		public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
			if (damageable.GetCurrentHealth() <= 0) {
				if (damageable is IBossEntity boss) {
					this.SendEvent<OnPlayerKillEnemy>(new OnPlayerKillEnemy() {
						DamageDealt = damage,
						Enemy = boss,
						IsBoss = true
					});
				}else if (damageable is IEnemyEntity enemy) {
					this.SendEvent<OnPlayerKillEnemy>(new OnPlayerKillEnemy() {
						DamageDealt = damage,
						Enemy = enemy,
						IsBoss = false
					});
				}
			}

			Debug.Log("Player Deal Damage to " + damageable.EntityName + " with damage " + damage);
		}

		
		public void RegisterOnModifyReceivedAddArmorAmount(Func<float, float> onModifyAddArmorAmount) {
			OnModifyAddArmorAmountCallbackList.Add(onModifyAddArmorAmount);
		}
		
		public void UnRegisterOnModifyReceivedAddArmorAmount(Func<float, float> onModifyAddArmorAmount) {
			OnModifyAddArmorAmountCallbackList.Remove(onModifyAddArmorAmount);
		}
	
		public ICanDealDamage ParentDamageDealer => null;

		public override void OnTakeDamage(int damage, ICanDealDamage damageDealer, HitData hitData = null) {
			base.OnTakeDamage(damage, damageDealer, hitData);
			AudioSystem.Singleton.Play2DSound("take_damage_flesh");

			if (GetCurrentHealth() <= 0)
			{
				AudioSystem.Singleton.Play2DSound("death");
				
				this.SendEvent<OnPlayerDie>(new OnPlayerDie() {
					damageDealer = damageDealer,
					hitData = hitData
				});
			}

			//Debug.Log("Player Take Damage from " + damageDealer.RootDamageDealer.EntityName + " with damage " + damage);
		}

		/*public ICanDealDamageRootEntity RootDamageDealer => this;

		public ICanDealDamageRootViewController RootViewController { get; protected set; } = null;*/

		protected override int DoTakeDamage(int actualDamage, [CanBeNull] ICanDealDamage damageDealer, [CanBeNull] HitData hitData, 
			bool nonlethal = false)  {
			
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			float armorToTakeDamage = Mathf.Min(Armor.Value, actualDamage);
			int healthToTakeDamage = actualDamage - (int) armorToTakeDamage;
			healthToTakeDamage = Mathf.Min(healthToTakeDamage, healthInfo.CurrentHealth);
			if(nonlethal && healthToTakeDamage >= healthInfo.CurrentHealth) {
				healthToTakeDamage = healthInfo.CurrentHealth - 1;
			}

			int totalDamage = (int) armorToTakeDamage + healthToTakeDamage;
			
			if (armorToTakeDamage > 0) {
				Armor.Value -= armorToTakeDamage;
				
				AudioSystem.Singleton.Play2DSound("armor_take_damage");
				
				if (Armor.Value == 0)
				{
					AudioSystem.Singleton.Play2DSound("armor_break");
				}
			}
				

			if (healthToTakeDamage > 0) {
				HealthProperty.RealValue.Value =
					new HealthInfo(healthInfo.MaxHealth, healthInfo.CurrentHealth - healthToTakeDamage);
				
			}
			
			this.SendEvent<OnPlayerTakeDamage>(new OnPlayerTakeDamage() {
				DamageTaken = totalDamage,
				HealthInfo = HealthProperty.RealValue.Value,
				HitData = hitData,
				DamageToHealth = healthToTakeDamage
			});
			
			
			return totalDamage;
			
		}

		public override void OnBuffUpdate(IBuff buff, BuffUpdateEventType eventType) {
			base.OnBuffUpdate(buff, eventType);
			this.SendEvent<OnPlayerBuffUpdate>(new OnPlayerBuffUpdate() {
				Buff = buff,
				EventType = eventType
			});
			
		}
	}
}