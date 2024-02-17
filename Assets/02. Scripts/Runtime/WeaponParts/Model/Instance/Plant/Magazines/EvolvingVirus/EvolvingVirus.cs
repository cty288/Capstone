using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Combat.Magazines.EvolvingVirus {
	public class EvolvingVirus : WeaponPartsEntity<EvolvingVirus, EvolvingVirusBuff> {
		public override string EntityName { get; set; } = "EvolvingVirus";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int time = GetCustomDataValueOfCurrentLevel<int>("time");
			return Localization.GetFormat(defaultLocalizationKey, time);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class EvolvingVirusBuff : WeaponPartsBuff<EvolvingVirus, EvolvingVirusBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<OnPlantBuffChangeDurationEvent>(OnModifyDurationEvent);
		}

		private OnPlantBuffChangeDurationEvent OnModifyDurationEvent(OnPlantBuffChangeDurationEvent e) {
			int time = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("time");
			e.Value += time;
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
			weaponEntity.UnRegisterOnModifyValueEvent<OnPlantBuffChangeDurationEvent>(OnModifyDurationEvent);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					int time = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("time");

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("EvolvingVirus_desc", time));
				})
			};
		}
	}
}