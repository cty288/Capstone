using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Attachments.SuddenDeath {
	public class SuddenDeath : WeaponPartsEntity<SuddenDeath, SuddenDeathBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "SuddenDeath";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float time = GetCustomDataValueOfCurrentLevel<float>("time");
			float damage = GetCustomDataValueOfCurrentLevel<float>("damage");
			int displayedDamage = (int) (damage * 100);

			return Localization.GetFormat(defaultLocalizationKey, time, displayedDamage);
		}


		public override int GetMinRarity() {
			return 2;
		}

		public override int GetMaxRarity() {
			return 4;
		}

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class SuddenDeathBuff : WeaponPartsBuff<SuddenDeath, SuddenDeathBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<OnPlantBuffChangeTotalDurationEvent>(OnPlantBuffChangeTotalDurationEvent);
			weaponEntity.RegisterOnModifyValueEvent<OnPlantBuffChangeTotalDamageEvent>(OnPlantBuffChangeTotalDamageEvent);
			weaponEntity.RegisterOnModifyValueEvent<OnPlantBuffChangeIsSuddenDeathEvent>(OnPlantBuffChangeIsSuddenDeathEvent);
		}

		private OnPlantBuffChangeIsSuddenDeathEvent OnPlantBuffChangeIsSuddenDeathEvent(OnPlantBuffChangeIsSuddenDeathEvent e) {
			e.Value = true;
			return e;
		}

		private OnPlantBuffChangeTotalDamageEvent OnPlantBuffChangeTotalDamageEvent(OnPlantBuffChangeTotalDamageEvent e) {
			float damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("damage");
			e.Value *= damage;
			return e;
		}

		private OnPlantBuffChangeTotalDurationEvent OnPlantBuffChangeTotalDurationEvent(OnPlantBuffChangeTotalDurationEvent e) {
			float time = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("time");
			e.Value = time;
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
			weaponEntity.UnRegisterOnModifyValueEvent<OnPlantBuffChangeTotalDamageEvent>(OnPlantBuffChangeTotalDamageEvent);
			weaponEntity.UnRegisterOnModifyValueEvent<OnPlantBuffChangeIsSuddenDeathEvent>(
				OnPlantBuffChangeIsSuddenDeathEvent);
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
					float damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("damage");
					int displayedDamage = (int) (damage * 100);

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("SuddenDeath_desc", time, displayedDamage));
				})
			};
		}
	}
}