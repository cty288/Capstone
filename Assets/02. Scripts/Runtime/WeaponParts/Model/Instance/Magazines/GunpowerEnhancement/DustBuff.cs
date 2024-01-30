using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Magazines.GunpowerEnhancement {
	public class DustBuff : Buff<DustBuff> {
		
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; } = -1;

		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = 0.1f;

		public override int Priority { get; } = 1;

		[field: ES3Serializable] private int maxLayer;
		[field: ES3Serializable] private int damage;
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

		public override void OnStacked(DustBuff buff) {
			this.maxLayer = Mathf.Max(buff.maxLayer, maxLayer);
			this.damage = Mathf.Max(buff.damage, damage);
			this.currentLayer = Mathf.Min(this.currentLayer + 1, maxLayer);
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			if (currentLayer < maxLayer) {
				return BuffStatus.Running;
			}

			damagableEntity.TakeDamage(damage, buffDealer as ICanDealDamage);
			return BuffStatus.End;
		}

		public override void OnEnds() {
			
		}
		public static DustBuff Allocate(int maxLayer, int damage, IEntity buffDealer, IEntity buffOwner) {
			DustBuff buff = Allocate(buffDealer, buffOwner);
			buff.maxLayer = maxLayer;
			buff.damage = damage;
			return buff;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			maxLayer = 0;
			damage = 0;
			currentLayer = 0;
		}
	}
}