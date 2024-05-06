using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using Polyglot;
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
		
		

		public override bool IsDisplayed() {
			return true;
		}

		public override bool Validate() {
			return base.Validate() && buffOwner is IDamageable;
		}

		public override void OnInitialize() {
			damagableEntity = buffOwner as IDamageable;
		}


		public override string GetLevelDescription(int level) {
			int LayerNumber = GetBuffPropertyAtLevel<int>( "layer_num", level);
			int Damage = GetBuffPropertyAtLevel<int>("damage", level);
			return Localization.GetFormat($"DustBuff_Desc", LayerNumber, Damage);
		}

		protected override void OnLevelUp() {
			
		}

		protected override void OnBuffStacked(DustBuff buff) {
			this.currentLayer++;
		}

		public override void OnStart() {
			this.currentLayer++;
		}

		public override BuffStatus OnTick() {
			int maxLayer = GetBuffPropertyAtCurrentLevel<int>("layer_num");
			if (currentLayer < maxLayer) {
				return BuffStatus.Running;
			}

			int damage = GetBuffPropertyAtCurrentLevel<int>("damage");
			damagableEntity.TakeDamage(damage, buffDealer as ICanDealDamage, out _);
			return BuffStatus.End;
		}

		

		public override void OnBuffEnd() {
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}


		public override void OnRecycled() {
			base.OnRecycled();
			currentLayer = 0;
		}

		public override string GetDisplayName(int level) {
			string name = base.GetDisplayName(level);
			if(currentLayer > 0) {
				return $"{name} ({currentLayer.ToString()})";
			}

			return name;
		}
	}
}