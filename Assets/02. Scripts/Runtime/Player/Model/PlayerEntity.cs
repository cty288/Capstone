using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MikroFramework.Architecture;
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
	public interface IPlayerEntity : ICreature, IEntity {
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
		
		IArmorProperty GetArmor();
		
		IArmorRecoverSpeedProperty GetArmorRecoverSpeed();
		
		IAirDrag GetAirDrag();
		MovementState GetMovementState();
		void SetMovementState(MovementState state);
		
		bool IsScopedIn();
		void SetScopedIn(bool state);
		
		void AddArmor(float amount);
		
	
		
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
		private IArmorProperty armor;
		private IArmorRecoverSpeedProperty armorRecoverSpeed;
		public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

		

		Action<IDamageable, int> ICanDealDamage.OnDealDamageCallback {
			get => _onDealDamageCallback;
			set => _onDealDamageCallback = value;
		}

		private MovementState movementState;
		private bool scopedIn;
		private Action<IDamageable, int> _onDealDamageCallback;
		private Action<IDamageable> _onKillDamageableCallback;

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
		}


		protected override void OnEntityStart(bool isLoadedFromSave) {
			
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
			armor = GetProperty<IArmorProperty>();
			armorRecoverSpeed = GetProperty<IArmorRecoverSpeedProperty>();
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

		public IArmorProperty GetArmor() {
			return armor;
		}
		
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
			float maxArmor = armor.InitialValue;
			if (armor.RealValue.Value < maxArmor) {
				float validAmount = Mathf.Min(maxArmor - armor.RealValue.Value, amount);
				armor.RealValue.Value += validAmount;
			}
		}

		


		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		protected override Faction GetDefaultFaction() {
			return Faction.Friendly;
		}

		public void OnKillDamageable(IDamageable damageable) {
			
		}

		public void OnDealDamage(IDamageable damageable, int damage) {
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

		Action<IDamageable> ICanDealDamage.OnKillDamageableCallback {
			get => _onKillDamageableCallback;
			set => _onKillDamageableCallback = value;
		}

		public ICanDealDamage ParentDamageDealer => null;

		public override void OnTakeDamage(int damage, ICanDealDamage damageDealer, HitData hitData = null) {
			base.OnTakeDamage(damage, damageDealer, hitData);
			if (GetCurrentHealth() <= 0) {
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
			float armorToTakeDamage = Mathf.Min(armor.RealValue.Value, actualDamage);
			int healthToTakeDamage = actualDamage - (int) armorToTakeDamage;
			healthToTakeDamage = Mathf.Min(healthToTakeDamage, healthInfo.CurrentHealth);
			if(nonlethal && healthToTakeDamage >= healthInfo.CurrentHealth) {
				healthToTakeDamage = healthInfo.CurrentHealth - 1;
			}

			int totalDamage = (int) armorToTakeDamage + healthToTakeDamage;
			
			if (armorToTakeDamage > 0) {
				armor.RealValue.Value -= armorToTakeDamage;
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