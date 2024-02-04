using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Magazines.GunpowerEnhancement {

	public class DustBuff : ConfigurableBuff<DustBuff> {
		
		public override bool IsGoodBuff => false;
		
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; } = -1;

		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = 0.1f;

		public override int Priority { get; } = 1;
		
		

		/*[field: ES3Serializable] private int maxLayer;
		[field: ES3Serializable] private int damage;*/
		[field: ES3Serializable] private int currentLayer;

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
		

		protected override void OnBuffStacked(DustBuff buff) {
			this.currentLayer++;
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			int maxLayer = GetBuffPropertyAtCurrentLevel<int>("layer_num");
			if (currentLayer < maxLayer) {
				return BuffStatus.Running;
			}

			int damage = GetBuffPropertyAtCurrentLevel<int>("damage");
			damagableEntity.TakeDamage(damage, buffDealer as ICanDealDamage);
			return BuffStatus.End;
		}

		public override void OnEnds() {
			
		}
	

		public override void OnRecycled() {
			base.OnRecycled();
			currentLayer = 0;
		}

		
	}
}