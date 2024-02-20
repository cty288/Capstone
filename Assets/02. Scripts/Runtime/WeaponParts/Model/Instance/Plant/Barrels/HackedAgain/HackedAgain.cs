using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Barrels.HackedAgain {
	public class HackedAgain : WeaponPartsEntity<HackedAgain, HackedAgainBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "HackedAgain";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int damage = GetCustomDataValueOfCurrentLevel<int>("damage");
			return Localization.GetFormat(defaultLocalizationKey, damage);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class HackedAgainBuff : WeaponPartsBuff<HackedAgain, HackedAgainBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			RegisterWeaponBuildBuffEvent<OnPlantBuffStackBuffEvent>(OnPlantBuffStackBuffEvent);
		}

		private void OnPlantBuffStackBuffEvent(OnPlantBuffStackBuffEvent e) {
			int damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
			e.Buff.DamagePerTick += damage;
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
			UnRegisterWeaponBuildBuffEvent<OnPlantBuffStackBuffEvent>(OnPlantBuffStackBuffEvent);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("HackedAgain_desc", damage));
				})
			};
		}
	}
}