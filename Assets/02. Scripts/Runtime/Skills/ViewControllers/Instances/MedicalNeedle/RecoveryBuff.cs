using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies.Model.Properties;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.MedicalNeedle {
	public class RecoveryBuff : Buff<RecoveryBuff> {
		public override float MaxDuration { get; protected set; } = 20;
		public override float TickInterval { get; protected set; } = 1;
		public override int Priority { get; } = 1;
		
		public override bool IsGoodBuff => true;
		
		[ES3Serializable]
		private int healAmountPerTick;
		
		private IDamageable damagableEntity;


		public override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.GetFormat(defaultLocalizationKey, healAmountPerTick, TickInterval);
		}

		public override bool IsDisplayed() {
			return true;
		}

		public override bool Validate() {
			return buffOwner is IDamageable;
		}

		public override void OnInitialize() {
			damagableEntity = buffOwner as IDamageable;
		}
		
		
		
		
		public override bool OnStacked(RecoveryBuff buff) {
			this.MaxDuration = Mathf.Max(this.MaxDuration, buff.MaxDuration);
			this.healAmountPerTick = Mathf.Max(this.healAmountPerTick, buff.healAmountPerTick);
			this.RemainingDuration = this.MaxDuration;
			this.TickInterval = Mathf.Min(this.TickInterval, buff.TickInterval);
			return true;
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			damagableEntity.Heal(healAmountPerTick, buffDealer as IBelongToFaction);
			return BuffStatus.Running;
		}
		

		public override void OnEnds() {
			
		}

		public static RecoveryBuff Allocate(IEntity buffDealer, IEntity entity, float maxDuration,
			int healAmountPerTick) {
			RecoveryBuff buff = RecoveryBuff.Allocate(buffDealer, entity);
			buff.MaxDuration = maxDuration;
			buff.healAmountPerTick = healAmountPerTick;
			return buff;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			healAmountPerTick = 0;
			damagableEntity = null;
		}
	}
}