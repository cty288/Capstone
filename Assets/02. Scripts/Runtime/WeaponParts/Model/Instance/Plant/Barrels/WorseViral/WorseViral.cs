using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Combat.Barrels.WorseViral {
	public class WorseViral : WeaponPartsEntity<WorseViral, WorseViralBuff> {
		public override string EntityName { get; set; } = "WorseViral";
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
	
	public class WorseViralBuff : WeaponPartsBuff<WorseViral, WorseViralBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<OnPlantBuffChangeDOTEvent>(OnCombatBuffChangeDOTEvent);
		}

		private OnPlantBuffChangeDOTEvent OnCombatBuffChangeDOTEvent(OnPlantBuffChangeDOTEvent e) {
			int damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
			e.Value += damage;
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
			weaponEntity.UnRegisterOnModifyValueEvent<OnPlantBuffChangeDOTEvent>(OnCombatBuffChangeDOTEvent);
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
						Localization.GetFormat("WorseViral_desc", damage));
				})
			};
		}
	}
}