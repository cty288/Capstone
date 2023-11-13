using JetBrains.Annotations;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.Collision;

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
		
		private OnHeal onHeal;

		private IHealthProperty healthProperty;
		public IHealthProperty HealthProperty => healthProperty;

		

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
		public void TakeDamage(int damage, [CanBeNull] ICanDealDamage damageDealer, [CanBeNull] HitData hitData = null, bool nonlethal = false) {
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			if(!CheckCanTakeDamage(damageDealer) || healthInfo.CurrentHealth <= 0) {
				return;
			}
			

			int actualDamage = OnTakeDamageAdditionalCheck(damage, damageDealer);
			
			//if curr health is less than damage, damage amount = curr health
			//else damage amount = damage
			int damageAmount = healthInfo.CurrentHealth < damage ? healthInfo.CurrentHealth : actualDamage;
			damageAmount = nonlethal && healthInfo.CurrentHealth <= damage ? healthInfo.CurrentHealth - 1 : damageAmount;
			DoTakeDamage(damageAmount);

			if (hitData != null) {
				hitData.Damage = damageAmount;
			}
			OnTakeDamage(damageAmount, damageDealer, hitData);
			damageDealer?.OnDealDamage(this, damageAmount);
			if (HealthProperty.RealValue.Value.CurrentHealth <= 0) {
				damageDealer?.OnKillDamageable(this);
			}
			
			onTakeDamage?.Invoke(damageAmount, HealthProperty.RealValue.Value.CurrentHealth, damageDealer, hitData);
			
		}

		/// <summary>
		/// Please use TakeDamage instead of this method
		/// </summary>
		/// <param name="damageAmount"></param>
		protected virtual void DoTakeDamage(int damageAmount) {
			HealthInfo healthInfo = HealthProperty.RealValue.Value;
			HealthProperty.RealValue.Value = new HealthInfo(healthInfo.MaxHealth, healthInfo.CurrentHealth - damageAmount);
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
			OnHeal(healAmountClamped, healer);
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

	}
}