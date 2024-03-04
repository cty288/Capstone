using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Player;
using Runtime.Temporary;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Time {
	public class MotivatedBuff : ConfigurableBuff<MotivatedBuff> {
		[field: ES3Serializable] public override float MaxDuration { get; protected set; } = 1;
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;
		public override int Priority => 1;
		 
		private ICanDealDamage owner;
		public override string GetLevelDescription(int level) {
			return GetDescription(level, "MotivatedBuff_Desc");
		}

		public override string OnGetDescription(string defaultLocalizationKey) {
			return GetDescription(Level, defaultLocalizationKey);
		}
		
		public static string GetDescription(int level, string localizationKey) {
			string additionalDescription = "";
			if (level >= 2) {
				additionalDescription = Localization.GetFormat("MotivatedBuff_Desc2",
					ConfigurableBuff<MotivatedBuff>.GetBuffPropertyAtLevel<int>("MotivatedBuff", "shield", level));
			}

			float damage =
				ConfigurableBuff<MotivatedBuff>.GetBuffPropertyAtLevel<float>("MotivatedBuff", "damage", level);

			int time = Mathf.RoundToInt(
				ConfigurableBuff<MotivatedBuff>.GetBuffPropertyAtLevel<float>("MotivatedBuff", "time", level));

			int displayedDamage = (int) (damage * 100);

			return Localization.GetFormat(localizationKey, level, displayedDamage, time, additionalDescription);
		}

		public override bool IsDisplayed() {
			return true;
		}

		public override bool Validate() {
			return base.Validate() && buffOwner is ICanDealDamage;
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override bool IsGoodBuff => true;
		
		public override void OnBuffEnd() {
		
		}

		public override void OnRecycled() {
			if (owner == null) {
				return;
			}
			
			owner.UnregisterOnModifyDamageCount(OnModifyDamageCount);
			owner.UnregisterOnKillDamageable(OnKillDamageable);
			owner = null;
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		private int OnModifyDamageCount(int count) {
			float percentage = GetBuffPropertyAtLevel<float>("damage", Level);
			return Mathf.CeilToInt(count * (1 + percentage));
		}

		public override void OnInitialize() {
			owner = (ICanDealDamage) buffOwner;
			if (owner == null) {
				return;
			}
			owner.RegisterOnModifyDamageCount(OnModifyDamageCount);
			owner.RegisterOnKillDamageable(OnKillDamageable);
		}

		private void OnKillDamageable(ICanDealDamage source, IDamageable target) {
			if (Level >= 2) {
				int shield = GetBuffPropertyAtLevel<int>("shield", Level);
				if (owner.CurrentFaction.Value != target.CurrentFaction.Value) {
					if(owner is IPlayerEntity player) {
						player.AddArmor(shield);
					}
				}
			}
		}

		protected override void OnLevelUp() {
			this.MaxDuration = GetBuffPropertyAtCurrentLevel<float>("time");
			this.RemainingDuration = MaxDuration;
		}

		protected override void OnBuffStacked(MotivatedBuff buff) {
			this.MaxDuration = Mathf.Max(buff.MaxDuration, this.MaxDuration);
			this.RemainingDuration = MaxDuration;
		}
		
		public new static MotivatedBuff Allocate(IEntity buffDealer, IEntity entity, int level) {
			MotivatedBuff buff = ConfigurableBuff<MotivatedBuff>.Allocate(buffDealer, entity, level);
			buff.MaxDuration = buff.GetBuffPropertyAtLevel<float>("time", level);
			return buff;
		}

		
	}
}