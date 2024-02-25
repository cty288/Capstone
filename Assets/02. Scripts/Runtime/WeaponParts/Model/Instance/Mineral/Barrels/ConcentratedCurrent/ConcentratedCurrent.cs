using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Barrels.ConcentratedCurrent {
	public class ConcentratedCurrent : WeaponPartsEntity<ConcentratedCurrent, ConcentratedCurrentBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "ConcentratedCurrent";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float range = GetCustomDataValueOfCurrentLevel<float>("range");
			int displayRange = (int) (range * 100);
			
			float damage = GetCustomDataValueOfCurrentLevel<float>("damage");
			int displayDamage = (int) (damage * 100);
			
			
			return Localization.GetFormat(defaultLocalizationKey, displayDamage, displayRange);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class ConcentratedCurrentBuff : WeaponPartsBuff<ConcentratedCurrent, ConcentratedCurrentBuff> {
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;

		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<MineralBuffMultiplierEvent>(OnModifyValueEvent);
			weaponEntity.RegisterOnModifyValueEvent<MineralBuffRangeMultiplierEvent>(OnModifyRangeEvent);
		}

		private MineralBuffRangeMultiplierEvent OnModifyRangeEvent(MineralBuffRangeMultiplierEvent e) {
			float range = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("range");
			e.Value *= (1 - range);
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
			weaponEntity.UnRegisterOnModifyValueEvent<MineralBuffRangeMultiplierEvent>(OnModifyRangeEvent);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {

					float range = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("range");
					int displayedRange = (int) (range * 100);
					
					float damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("damage");
					int displayedMultiplier = (int) (damage * 100);

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("ConcentratedCurrent_desc", displayedMultiplier, displayedRange));
				})
			};
		}
	}
}