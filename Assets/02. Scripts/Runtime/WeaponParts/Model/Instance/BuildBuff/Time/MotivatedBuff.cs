using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Time {
	public class MotivatedBuff : ConfigurableBuff<MotivatedBuff> {
		[field: ES3Serializable] public override float MaxDuration { get; protected set; } = 1;
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;
		public override int Priority => 1;
		 
		public override string OnGetDescription(string defaultLocalizationKey) {
			string additionalDescription = "";
			if (Level >= 2) {
				additionalDescription = Localization.GetFormat("MotivatedBuff_Desc_2", 
					GetBuffPropertyAtLevel<int>("shield", Level));
			}
			
			float damage = GetBuffPropertyAtLevel<float>("damage", Level);
			int displayedDamage = (int) damage * 100;
			return Localization.GetFormat(defaultLocalizationKey, displayedDamage, additionalDescription);
		}

		public override bool IsDisplayed() {
			return true;
		}

		public override bool Validate() {
			return buffOwner is ICanDealDamage;
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override bool IsGoodBuff => true;
		public override void OnEnds() {
			
		}

		public override void OnInitialize() {
			
		}

		protected override void OnBuffStacked(MotivatedBuff buff) {
			this.Level = Mathf.Max(buff.Level, this.Level);
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