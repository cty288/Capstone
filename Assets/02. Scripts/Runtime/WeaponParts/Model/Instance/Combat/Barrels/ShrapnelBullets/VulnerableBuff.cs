using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Combat.Barrels.ShrapnelBullets {
	public class VulnerableBuff : ConfigurableBuff<VulnerableBuff> {
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; }
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; }
		[field: ES3Serializable] public override int Priority { get; } = 1;
		
		public override bool IsGoodBuff => false;
		
		private IDamageable damagableEntity;
		public override bool IsDisplayed() {
			return true;
		}

		public override void OnInitialize() {
			damagableEntity = buffOwner as IDamageable;
			
			if (damagableEntity == null) {
				return;
			}

			damagableEntity.RegisterOnModifyReceivedDamage(OnModifyDamage);
		}

		private int OnModifyDamage(int damage, ICanDealDamage dealer) {
			float damageMultiplier = GetBuffPropertyAtCurrentLevel<float>("buff_damage");
			return Mathf.RoundToInt(damage * (1 + damageMultiplier));
		}

		public override bool Validate() {
			return base.Validate() && buffOwner is IDamageable;
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		
		public override void OnBuffEnd() {
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override string GetLevelDescription(int level) {
			float damage = GetBuffPropertyAtLevel<float>("buff_damage", level);
			int displayedDamage = Mathf.RoundToInt(damage * 100);

			int time = GetBuffPropertyAtLevel<int>("buff_length", level);
			return Localization.GetFormat("VulnerableBuff_Desc", level, displayedDamage, time);
		}

		protected override void OnLevelUp() {
			this.MaxDuration = GetBuffPropertyAtCurrentLevel<float>("buff_length");
			this.TickInterval = GetBuffPropertyAtCurrentLevel<float>("tick_interval");
		}

		protected override void OnBuffStacked(VulnerableBuff buff) {
			this.MaxDuration = Mathf.Max(this.MaxDuration, buff.MaxDuration);
			this.RemainingDuration = this.MaxDuration;
		}
		
		public static VulnerableBuff Allocate(int level, IEntity buffDealer, IEntity buffOwner) {
			VulnerableBuff buff = Allocate(buffDealer, buffOwner, level);
			buff.MaxDuration = buff.GetBuffPropertyAtCurrentLevel<float>("buff_length");
			return buff;
		}


		public override void OnRecycled() {
			if (damagableEntity == null) {
				return;
			}
			damagableEntity.UnRegisterOnModifyReceivedDamage(OnModifyDamage);
			damagableEntity = null;
			base.OnRecycled();
		}
	}
}