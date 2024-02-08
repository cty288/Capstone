﻿using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.Model.Properties;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialBarrel {
	public class BleedingBuff : ConfigurableBuff<BleedingBuff> {
		public override bool IsGoodBuff => false;
		
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; }
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; }
		[field: ES3Serializable] public override int Priority { get; } = 1;
		//[field: ES3Serializable] private float damage;
		
		
		private IDamageable damagableEntity;
		
		public override bool IsDisplayed() {
			return true;
		}
		public override string GetLevelDescription(int level) {
			return null;
		}
		public override bool Validate() {
			return base.Validate() && buffOwner is IDamageable;
		}

		public override void OnInitialize() {
			damagableEntity = buffOwner as IDamageable;
		}


		protected override void OnLevelUp() {
			this.MaxDuration = GetBuffPropertyAtCurrentLevel<float>("buff_length");
			this.TickInterval = GetBuffPropertyAtCurrentLevel<float>("tick_interval");
			this.RemainingDuration = this.MaxDuration;
		}

		protected override void OnBuffStacked(BleedingBuff buff) {
			this.MaxDuration = Mathf.Max(this.MaxDuration, buff.MaxDuration);
			this.RemainingDuration = this.MaxDuration;
			this.TickInterval = Mathf.Min(this.TickInterval, buff.TickInterval);
		}


		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {

			float damage = GetBuffPropertyAtCurrentLevel<float>("buff_damage");
			
			if (Level <= 2) {
				damagableEntity.TakeDamage(Mathf.RoundToInt(damage), buffDealer as ICanDealDamage);
			}else {
				int maxHealth = damagableEntity.HealthProperty.InitialValue.MaxHealth;
				int damageRounded = Mathf.RoundToInt(Mathf.Max(1, damage * maxHealth));

				HealthInfo healthInfo = damagableEntity.HealthProperty.RealValue.Value;
				damagableEntity.HealthProperty.RealValue.Value =
					new HealthInfo(healthInfo.MaxHealth - damageRounded, healthInfo.CurrentHealth);
				damagableEntity.TakeDamage(damageRounded, buffDealer as ICanDealDamage);
			}

			return BuffStatus.Running;
		}

		

		public override void OnBuffEnd() {
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public static BleedingBuff Allocate(float tickInterval, int level, IEntity buffDealer, IEntity buffOwner) {
			BleedingBuff buff = Allocate(buffDealer, buffOwner, level);
			buff.MaxDuration = buff.GetBuffPropertyAtCurrentLevel<float>("buff_length");
			buff.TickInterval = tickInterval;
			return buff;
		}
	}
}