using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant {
	public class PowerlessBuff  : ConfigurableBuff<PowerlessBuff> {
		[field: ES3Serializable] public override float MaxDuration { get; protected set; } = 1;
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;
		[field: ES3Serializable] public override int Priority { get; } = 1;

		//[ES3Serializable] private float damageMultiplier;
		private ICanDealDamage owner;
		

		public override bool IsDisplayed() {
			return true;
		}
		
		
		public override string GetLevelDescription(int level) {
			int displayedPercentage = Mathf.RoundToInt(GetBuffPropertyAtLevel<float>("damage_multiplier", level) * 100);
			return Localization.GetFormat("PowerlessBuff_Desc", level, displayedPercentage);
		}
		
		
		public override void OnInitialize() {
			owner = (ICanDealDamage) buffOwner;
			if (owner == null) {
				return;
			}
			owner.RegisterOnModifyDamageCount(OnModifyDamageCount);
		}

		private int OnModifyDamageCount(int count) {
			float percentage = GetBuffPropertyAtLevel<float>("damage_multiplier", Level);
			return Mathf.RoundToInt(count * (1 - percentage));
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override bool IsGoodBuff => true;
		public override void OnBuffEnd() {
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		protected override void OnLevelUp() {
			
		}

		protected override void OnBuffStacked(PowerlessBuff buff) {
			this.MaxDuration = Mathf.Max(this.MaxDuration, buff.MaxDuration);
			this.RemainingDuration = MaxDuration;
		}
		
		public new static PowerlessBuff Allocate(IEntity buffDealer, IEntity entity, int level, float time) {
			PowerlessBuff buff = ConfigurableBuff<PowerlessBuff>.Allocate(buffDealer, entity, level);
			buff.MaxDuration = time;
			//buff.damageMultiplier = GetBuffPropertyAtLevel<float>("PowerlessBuff", "damage_multiplier", level);
			
			return buff;
		}

		public override bool Validate() {
			return base.Validate() && buffOwner is ICanDealDamage;
		}

		public override void OnRecycled() {
			if (owner == null) {
				return;
			}
			owner.UnregisterOnModifyDamageCount(OnModifyDamageCount);
			owner = null;
			base.OnRecycled();
		}
	}
}