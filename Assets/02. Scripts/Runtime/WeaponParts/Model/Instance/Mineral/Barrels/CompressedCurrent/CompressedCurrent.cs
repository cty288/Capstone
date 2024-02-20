using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Barrels.CompressedCurrent {
	public class CompressedCurrent : WeaponPartsEntity<CompressedCurrent, CompressedCurrentBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "CompressedCurrent";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float damage = GetCustomDataValueOfCurrentLevel<float>("damage");
			int displayedMultiplier = (int) (damage * 100);
			
			return Localization.GetFormat(defaultLocalizationKey, displayedMultiplier);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class CompressedCurrentBuff : WeaponPartsBuff<CompressedCurrent, CompressedCurrentBuff> {
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;

		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<MineralBuffMultiplierEvent>(OnModifyValueEvent);
			weaponEntity.RegisterOnModifyValueEvent<OnModifyMineralAOEIndividualDamage>(OnModifyMineralAOEIndividualDamage);
			
		}

		private OnModifyMineralAOEIndividualDamage OnModifyMineralAOEIndividualDamage(OnModifyMineralAOEIndividualDamage e) {
			float range = e.AOERange;
			float distance = e.Distance;
			
			//make damage decrease with far distance, distance is 0 when the target is at the center of the explosion
			//also multiplayer minimum damage is 0.1
			e.Value = Mathf.CeilToInt(e.Value * Mathf.Max(0.1f, 1 - distance / range));
			return e;
		}


		private MineralBuffMultiplierEvent OnModifyValueEvent(MineralBuffMultiplierEvent e) {
			float multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("damage");
			e.Value *= (1 + multiplier);
			return e;
		}



		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {

		}

		public override void OnRecycled() {
			weaponEntity.UnRegisterOnModifyValueEvent<MineralBuffMultiplierEvent>(OnModifyValueEvent);
			weaponEntity.UnRegisterOnModifyValueEvent<OnModifyMineralAOEIndividualDamage>(OnModifyMineralAOEIndividualDamage);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					float damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("damage");
					int displayedMultiplier = (int) (damage * 100);

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("CompressedCurrent_desc", displayedMultiplier));
				})
			};
		}
	}
}