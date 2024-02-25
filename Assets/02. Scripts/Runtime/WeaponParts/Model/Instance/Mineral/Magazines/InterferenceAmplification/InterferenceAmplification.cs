using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Barrels.ConcentratedCurrent;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Magazines.InterferenceAmplification {
	public class InterferenceAmplification : WeaponPartsEntity<InterferenceAmplification, InterferenceAmplificationBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "InterferenceAmplification";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int range = GetCustomDataValueOfCurrentLevel<int>("range");
			
			
			return Localization.GetFormat(defaultLocalizationKey, range);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class InterferenceAmplificationBuff : WeaponPartsBuff<InterferenceAmplification, InterferenceAmplificationBuff> {
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;

		public override void OnInitialize() {
			
			weaponEntity.RegisterOnModifyValueEvent<MineralBuffRangeEvent>(OnModifyRangeEvent);
		}

		private MineralBuffRangeEvent OnModifyRangeEvent(MineralBuffRangeEvent e) {
			int range = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("range");
			e.Value += range;
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
			weaponEntity.UnRegisterOnModifyValueEvent<MineralBuffRangeEvent>(OnModifyRangeEvent);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {

					int range = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("range");

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("InterferenceAmplification_desc", range));
				})
			};
		}
	}
}