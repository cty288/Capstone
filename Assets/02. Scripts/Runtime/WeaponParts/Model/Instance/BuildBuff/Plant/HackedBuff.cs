using _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialBarrel;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant {
	public class HackedBuff : Buff<HackedBuff> {
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; }

		[field: ES3Serializable] public override float TickInterval { get; protected set; } = 1;
		[field: ES3Serializable] public override int Priority { get; } = 1;
		private IDamageable damagableEntity;
		
		[ES3Serializable]
		private float damage;

		[ES3Serializable] private float damageMultiplier = 1;
		
		[ES3Serializable] private bool isSuddenDeathBuff;
		[ES3Serializable] private int suddenDeathBuffDamage;
		
		
		public override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.GetFormat(defaultLocalizationKey, damage * damageMultiplier);
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
			OnTick();
		}

		public override BuffStatus OnTick() {
			if (!isSuddenDeathBuff) {
				damagableEntity.TakeDamage(Mathf.RoundToInt(damage * damageMultiplier), buffDealer as ICanDealDamage, out _);
			}
			
			return BuffStatus.Running;
		}

		public override bool IsGoodBuff => false;
		public override void OnEnds() {
			if (isSuddenDeathBuff) {
				damagableEntity.TakeDamage(suddenDeathBuffDamage, buffDealer as ICanDealDamage, out _);
			}
		}
		
		public static HackedBuff Allocate(IEntity buffDealer, IEntity buffOwner, float totalDuration, int damage, float damageMultiplier,
			bool isSuddenDeathBuff, int suddenDeathBuffDamage) {
			HackedBuff buff = Allocate(buffDealer, buffOwner);
			buff.MaxDuration = totalDuration;
			buff.damage = damage;
			buff.damageMultiplier = damageMultiplier;
			buff.isSuddenDeathBuff = isSuddenDeathBuff;
			buff.suddenDeathBuffDamage = suddenDeathBuffDamage;
			return buff;
		}
	}
}