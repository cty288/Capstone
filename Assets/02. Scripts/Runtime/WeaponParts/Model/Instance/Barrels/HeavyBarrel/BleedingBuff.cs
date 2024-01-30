using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.Model.Properties;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialBarrel {
	public class BleedingBuff : Buff<BleedingBuff> {
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; }
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; }

		[field: ES3Serializable] public override int Priority { get; } = 1;

		[field: ES3Serializable] private float damage;
		[field: ES3Serializable] private int level;
		
		private IDamageable damagableEntity;
		public override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}

		public override bool IsDisplayed() {
			return false;
		}

		public override bool Validate() {
			return buffOwner is IDamageable;
		}

		public override void OnInitialize() {
			damagableEntity = buffOwner as IDamageable;
		}

		public override void OnStacked(BleedingBuff buff) {
			this.MaxDuration = Mathf.Max(this.MaxDuration, buff.MaxDuration);
			this.RemainingDuration = this.MaxDuration;
			this.TickInterval = Mathf.Min(this.TickInterval, buff.TickInterval);
			if (buff.level > this.level) {
				this.level = buff.level;
				this.damage = buff.damage;
			}
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			
			if (level <= 2) {
				damagableEntity.TakeDamage(Mathf.RoundToInt(damage), buffDealer as ICanDealDamage);
			}
			else {
				int maxHealth = damagableEntity.HealthProperty.InitialValue.MaxHealth;
				int damage = Mathf.RoundToInt(Mathf.Max(1, this.damage * maxHealth));

				HealthInfo healthInfo = damagableEntity.HealthProperty.RealValue.Value;
				damagableEntity.HealthProperty.RealValue.Value =
					new HealthInfo(healthInfo.MaxHealth - damage, healthInfo.CurrentHealth);
				damagableEntity.TakeDamage(damage, buffDealer as ICanDealDamage);
			}

			return BuffStatus.Running;
		}

		public override void OnEnds() {
			
		}
		
		public static BleedingBuff Allocate(float maxDuration, float tickInterval, float damage, int level, IEntity buffDealer, IEntity buffOwner) {
			BleedingBuff buff = Allocate(buffDealer, buffOwner);
			buff.MaxDuration = maxDuration;
			buff.TickInterval = tickInterval;
			buff.damage = damage;
			buff.level = level;
			return buff;
		}
	}
}