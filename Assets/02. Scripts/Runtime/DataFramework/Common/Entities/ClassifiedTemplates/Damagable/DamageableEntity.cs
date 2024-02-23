using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable {
	
	
	
	/// <summary>
	/// Health is auto registered
	/// </summary>
	public abstract class DamageableEntity : AbstractBasicEntity, IDamageable {
		
		[field: ES3Serializable]
		public BindableProperty<Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction>();
		
		[field: ES3Serializable]
		public BindableProperty<bool> IsInvincible { get; } = new BindableProperty<bool>();



		private OnTakeDamage onTakeDamage;
		private Action<ICanDealDamage, IDamageable, HitData> onDie;
		
		private OnHeal onHeal;

		private IHealthProperty healthProperty;
		public IHealthProperty HealthProperty => healthProperty;


		private HashSet<Func<int, ICanDealDamage, int>> onModifyDamageCountCallbackList =
			new HashSet<Func<int, ICanDealDamage, int>>();

		private HashSet<Func<int, IBelongToFaction, IDamageable, int>> onModifyHealAmountCallbackList =
			new HashSet<Func<int, IBelongToFaction, IDamageable, int>>();

		public override void OnAwake() {
			base.OnAwake();
			this.healthProperty = GetProperty<IHealthProperty>();
			CurrentFaction.Value = GetDefaultFaction();
		}

		public DamageableEntity() : base() {
			//healthProperty = this.GetProperty<IHealthProperty>();
			
		}

		protected override void OnRegisterProperties() {
			base.OnRegisterProperties();
			RegisterInitialProperty<IHealthProperty>(new Health());
		}

		protected abstract Faction GetDefaultFaction();

		public int GetMaxHealth() {
			return HealthProperty.GetMaxHealth();
		}

		public int GetCurrentHealth() {
			return HealthProperty.GetCurrentHealth();
		}
		

		/// <summary>
		/// Will not take damage if the damage dealer is in the same faction <br/>
		/// Will take 0 damage if the entity is invincible
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="damageDealer"></param>
		public void TakeDamage(int damage, [CanBeNull] ICanDealDamage damageDealer, out bool isDie, [CanBeNull] HitData hitData = null, bool nonlethal = false) {
			isDie = false;
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			if(!CheckCanTakeDamage(damageDealer) || healthInfo.CurrentHealth <= 0) {
				return;
			}


			int modifiedDamage = damageDealer?.DoModifyDamageCount(damage) ?? damage;
			foreach (var onModifyDamage in onModifyDamageCountCallbackList) {
				modifiedDamage = onModifyDamage(modifiedDamage, damageDealer);
			}
			
			int actualDamage = OnTakeDamageAdditionalCheck(modifiedDamage, damageDealer);
			
			//if curr health is less than damage, damage amount = curr health
			//else damage amount = damage
			
			
			int damageAmount = DoTakeDamage(actualDamage, damageDealer, hitData);

			if (hitData != null) {
				hitData.Damage = damageAmount;
			}
			OnTakeDamage(damageAmount, damageDealer, hitData);
			damageDealer?.DoOnDealDamage( damageDealer,this, damageAmount);
			
			
			onTakeDamage?.Invoke(damageAmount, HealthProperty.RealValue.Value.CurrentHealth, damageDealer, hitData);
			
			if (HealthProperty.RealValue.Value.CurrentHealth <= 0) {
				Kill(damageDealer, hitData);
				isDie = true;
			}
		}

		public void Kill(ICanDealDamage damageDealer, HitData hitData = null) {
			damageDealer?.DoOnKillDamageable(damageDealer, this);
			onDie?.Invoke(damageDealer, this, hitData);
		}

		/// <summary>
		/// Please use TakeDamage instead of this method
		/// </summary>
		/// <param name="damageAmount"></param>
		protected virtual int DoTakeDamage(int actualDamage, [CanBeNull] ICanDealDamage damageDealer, [CanBeNull] HitData hitData, bool nonlethal = false) {
			int damageAmount = healthProperty.GetCurrentHealth() < actualDamage ? healthProperty.GetCurrentHealth() : actualDamage;
			damageAmount = nonlethal && healthProperty.GetCurrentHealth() <= actualDamage ? healthProperty.GetCurrentHealth() - 1 : damageAmount;
			
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			HealthProperty.RealValue.Value = new HealthInfo(healthInfo.MaxHealth, healthInfo.CurrentHealth - damageAmount);
			
			return damageAmount;
		}
		
		
		public virtual void OnTakeDamage(int damage, [CanBeNull] ICanDealDamage damageDealer, [CanBeNull] HitData hitData = null) {
			
		}
		
		
		
		
		public virtual int OnTakeDamageAdditionalCheck(int damage, [CanBeNull] IBelongToFaction damageDealer) {
			if(IsInvincible.Value) {
				damage = 0;
			}

			return damage;
		}
		
		public bool CheckCanTakeDamage([CanBeNull] ICanDealDamage damageDealer) {
			if(damageDealer != null && damageDealer.IsSameFaction(this)) {
				return false;
			}

			return true;
		}

		public void RegisterOnModifyReceivedDamage(Func<int, ICanDealDamage, int> onModifyDamage) {
			onModifyDamageCountCallbackList.Add(onModifyDamage);
		}

		public void UnRegisterOnModifyReceivedDamage(Func<int, ICanDealDamage, int> onModifyDamage) {
			onModifyDamageCountCallbackList.Remove(onModifyDamage);
		}

		public IUnRegister RegisterOnTakeDamage(OnTakeDamage onTakeDamage) {
			this.onTakeDamage += onTakeDamage;
			return new OnTakeDamageUnRegister(this, onTakeDamage);
		}

		public void UnRegisterOnTakeDamage(OnTakeDamage onTakeDamage) {
			this.onTakeDamage -= onTakeDamage;
		}

		public void RegisterOnDie(Action<ICanDealDamage, IDamageable, HitData> onDie) {
			this.onDie += onDie;
		}

		public void UnRegisterOnDie(Action<ICanDealDamage, IDamageable, HitData> onDie) {
			this.onDie -= onDie;
		}


		public void Heal(int healAmount, [CanBeNull] IBelongToFaction healer) {
			//if curr health + heal amount is greater than max health, heal amount = max health - curr health
			//else heal amount = heal amount
			foreach (var onModifyHealAmount in onModifyHealAmountCallbackList) {
				healAmount = onModifyHealAmount(healAmount, healer, this);
			}
			
			int maxHealth = HealthProperty.GetMaxHealth();
			int currentHealth = HealthProperty.GetCurrentHealth();
			int healAmountClamped = currentHealth + healAmount > maxHealth ? maxHealth - currentHealth : healAmount;
			
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			HealthProperty.RealValue.Value = new HealthInfo(healthInfo.MaxHealth, healthInfo.CurrentHealth + healAmountClamped);

			onHeal?.Invoke(healAmountClamped, HealthProperty.RealValue.Value.CurrentHealth, healer);
			OnHeal(healAmountClamped, healer);
		}
		
		public void SetHealth(int health) {
			int actualHealth = Mathf.Clamp(health, 0, HealthProperty.GetMaxHealth());
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			HealthProperty.RealValue.Value = new HealthInfo(healthInfo.MaxHealth, actualHealth);
			if (HealthProperty.RealValue.Value.CurrentHealth <= 0) {
				Kill(null, null);
			}
		}

		public void RegisterOnModifyReceivedHealAmount(Func<int, IBelongToFaction, IDamageable, int> onModifyHealAmount) {
			onModifyHealAmountCallbackList.Add(onModifyHealAmount);
		}

		public void UnRegisterOnModifyReceivedHealAmount(Func<int, IBelongToFaction, IDamageable, int> onModifyHealAmount) {
			onModifyHealAmountCallbackList.Remove(onModifyHealAmount);
		}

		


		public virtual void OnHeal(int healAmount, [CanBeNull] IBelongToFaction healer) {
			
		}

		public IUnRegister RegisterOnHeal(OnHeal onHeal) {
			this.onHeal += onHeal;
			return new OnHealUnRegister(this, onHeal);
		}
		
		public void UnRegisterOnHeal(OnHeal onHeal) {
			this.onHeal -= onHeal;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			onTakeDamage = null;
			onDie = null;
			onHeal = null;
			onModifyDamageCountCallbackList.Clear();
			onModifyHealAmountCallbackList.Clear();
		}
	}
}