using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Magazines {
	public class CustomizedMagazine : WeaponPartsEntity<CustomizedMagazine, CustomizedMagazineBuff> {
		public override string EntityName { get; set; } = "CustomizedMagazine";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("multiplier");
			int displayMultiplayer = (int) (multiplayer);
			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class CustomizedMagazineBuff : WeaponPartsBuff<CustomizedMagazine, CustomizedMagazineBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		[ES3Serializable]
		private float multiplier;
		
		[ES3Serializable]
		private Vector2Int addedDamage = new Vector2Int(0, 0);
		public override void OnInitialize() {
			weaponEntity.CurrentAmmo.RegisterWithInitValue(OnAmmoChange);
		}

		private void OnAmmoChange(int arg1, int newAmmo) {
			if (newAmmo == 1 && addedDamage == Vector2Int.zero) {
				Vector2Int baseDamage = weaponEntity.GetBaseDamage().InitialValue;
				addedDamage = baseDamage * (int) multiplier - baseDamage;
				weaponEntity.GetBaseDamage().RealValue.Value += addedDamage;
			}
			else {
				weaponEntity.GetBaseDamage().RealValue.Value -= addedDamage;
				addedDamage = Vector2Int.zero;
			}
		}

		
		public override void OnStart() {
			base.OnStart();
			multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		public override void OnRecycled() {
			weaponEntity.CurrentAmmo.UnRegisterOnValueChanged(OnAmmoChange);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("CustomizedMagazine_desc", (int) multiplier));
				})
			};
		}
	}
}