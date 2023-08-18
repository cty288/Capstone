using JetBrains.Annotations;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Faction;
using Runtime.DataFramework.Properties;
using UnityEngine;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable {
	
	
	
	/// <summary>
	/// Health is auto registered
	/// </summary>
	public abstract class DamagableEntity : AbstractBasicEntity, IDamagable {
		
		[field: ES3Serializable]
		public BindableProperty<Faction.Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction.Faction>();
		
		[field: ES3Serializable]
		public BindableProperty<bool> IsInvincible { get; } = new BindableProperty<bool>();
		
		private OnTakeDamage onTakeDamage;
		
		private OnHeal onHeal;

		public IHealthProperty HealthProperty => GetProperty<IHealthProperty>();

	//	private IHealthProperty healthProperty;
		
		
		//TODO: rebind health

		public DamagableEntity() : base() {
			//healthProperty = this.GetProperty<IHealthProperty>();
			CurrentFaction.Value = GetDefaultFaction();
		}

		protected override void OnRegisterProperties() {
			base.OnRegisterProperties();
			RegisterInitialProperty<IHealthProperty>(new Health());
		}

		protected abstract Faction.Faction GetDefaultFaction();

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
		public void TakeDamage(int damage, [CanBeNull] IBelongToFaction damageDealer) {
			if(damageDealer != null && damageDealer.CurrentFaction == CurrentFaction.Value) {
				return;
			}
			
			if(IsInvincible.Value) {
				damage = 0;
			}
			
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			
			//if curr health is less than damage, damage amount = curr health
			//else damage amount = damage
			int damageAmount = healthInfo.CurrentHealth < damage ? healthInfo.CurrentHealth : damage;
			HealthProperty.RealValue.Value = new HealthInfo(healthInfo.MaxHealth, healthInfo.CurrentHealth - damageAmount);

				onTakeDamage?.Invoke(damageAmount, HealthProperty.RealValue.Value.CurrentHealth, damageDealer);
		}

		public IUnRegister RegisterOnTakeDamage(OnTakeDamage onTakeDamage) {
			this.onTakeDamage += onTakeDamage;
			return new OnTakeDamageUnRegister(this, onTakeDamage);
		}

		public void UnRegisterOnTakeDamage(OnTakeDamage onTakeDamage) {
			this.onTakeDamage -= onTakeDamage;
		}


		public void Heal(int healAmount, [CanBeNull] IBelongToFaction healer) {
			//if curr health + heal amount is greater than max health, heal amount = max health - curr health
			//else heal amount = heal amount
			
			int maxHealth = HealthProperty.GetMaxHealth();
			int currentHealth = HealthProperty.GetCurrentHealth();
			int healAmountClamped = currentHealth + healAmount > maxHealth ? maxHealth - currentHealth : healAmount;
			
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			HealthProperty.RealValue.Value = new HealthInfo(healthInfo.MaxHealth, healthInfo.CurrentHealth + healAmountClamped);

			onHeal?.Invoke(healAmountClamped, HealthProperty.RealValue.Value.CurrentHealth, healer);
		}

		public IUnRegister RegisterOnHeal(OnHeal onHeal) {
			this.onHeal += onHeal;
			return new OnHealUnRegister(this, onHeal);
		}
		
		public void UnRegisterOnHeal(OnHeal onHeal) {
			this.onHeal -= onHeal;
		}

	}
}