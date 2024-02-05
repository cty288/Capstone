using _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialBarrel;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Mineral {
	public class HackedBuff : Buff<HackedBuff> {
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; }

		[field: ES3Serializable] public override float TickInterval { get; protected set; } = 1;
		[field: ES3Serializable] public override int Priority { get; } = 1;
		private IDamageable damagableEntity;
		
		[ES3Serializable]
		private float damage;

		[ES3Serializable] private float damageMultiplier = 1;
		
		public override string OnGetDescription(string defaultLocalizationKey) {
			return null;
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

		public override bool OnStacked(HackedBuff buff) {
			return false;
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			damagableEntity.TakeDamage(Mathf.RoundToInt(damage * damageMultiplier), buffDealer as ICanDealDamage);
			return BuffStatus.Running;
		}

		public override bool IsGoodBuff => false;
		public override void OnEnds() {
			
		}
		
		public static HackedBuff Allocate(IEntity buffDealer, IEntity buffOwner, float totalDuration, int damage, float damageMultiplier) {
			HackedBuff buff = Allocate(buffDealer, buffOwner);
			buff.MaxDuration = totalDuration;
			buff.damage = damage;
			buff.damageMultiplier = damageMultiplier;
			return buff;
		}
	}
}