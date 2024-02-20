using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Magazines.FierceVirus {
	public class FierceVirus : WeaponPartsEntity<FierceVirus, FierceVirusBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "FierceVirus";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override int GetMaxRarity() {
			return 3;
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float time = GetCustomDataValueOfCurrentLevel<float>("time");
			int displayedTime = (int) (time * 100);
			return Localization.GetFormat(defaultLocalizationKey, displayedTime);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class FierceVirusBuff : WeaponPartsBuff<FierceVirus, FierceVirusBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<OnPlantBuffChangeTotalDurationEvent>(OnPlantBuffChangeTotalDurationEvent);
		}

		private OnPlantBuffChangeTotalDurationEvent OnPlantBuffChangeTotalDurationEvent(OnPlantBuffChangeTotalDurationEvent e) {
			float time = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("time");
			e.Value *= (1 - time);
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
			weaponEntity.UnRegisterOnModifyValueEvent<OnPlantBuffChangeTotalDurationEvent>(OnPlantBuffChangeTotalDurationEvent);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					float time = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("time");
					int displayedTime = (int) (time * 100);

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("FierceVirus_desc", displayedTime));
				})
			};
		}
	}
}