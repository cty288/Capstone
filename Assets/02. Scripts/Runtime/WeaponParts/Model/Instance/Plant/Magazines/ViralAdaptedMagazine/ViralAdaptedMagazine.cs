using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Magazines.ViralAdaptedMagazine {
	public class ViralAdaptedMagazine : WeaponPartsEntity<ViralAdaptedMagazine, ViralAdaptedMagazineBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "ViralAdaptedMagazine";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float ammo = GetCustomDataValueOfCurrentLevel<float>("ammo");
			int displayAmmo = (int) (ammo * 100);
			return Localization.GetFormat(defaultLocalizationKey, displayAmmo);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class ViralAdaptedMagazineBuff : WeaponPartsBuff<ViralAdaptedMagazine, ViralAdaptedMagazineBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		private BuffedProperties<int> ammoSizeProperties;
		[ES3Serializable] private int addedAmmo;
		public override void OnInitialize() {
			ammoSizeProperties = new BuffedProperties<int>(weaponEntity, true, BuffTag.Weapon_AmmoSize);
		}
		
		

		public override void OnStart() {
			base.OnStart();
			float ammo = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("ammo");
		

			IBuffedProperty<int> ammoSizeProperty = ammoSizeProperties.Properties.First();
			addedAmmo = (Mathf.RoundToInt((ammoSizeProperty.BaseValue * ammo)));
			ammoSizeProperty.RealValue.Value += addedAmmo;
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			IBuffedProperty<int> ammoSizeProperty = ammoSizeProperties.Properties.First();
			ammoSizeProperty.RealValue.Value -= addedAmmo;
		}


		public override void OnRecycled() {
			base.OnRecycled();
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return new[] {ammoSizeProperties};
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return null;
		}
	}
}