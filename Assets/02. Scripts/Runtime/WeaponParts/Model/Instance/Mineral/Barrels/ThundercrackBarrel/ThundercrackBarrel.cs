using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Barrels {
	public class ThundercrackBarrel : WeaponPartsEntity<ThundercrackBarrel, ThundercrackBarrelBuff> {
		public override string EntityName { get; set; } = "ThundercrackBarrel";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("multiplier");
			int displayMultiplayer = (int) (multiplayer * 100);
			
			
			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class ThundercrackBarrelBuff : WeaponPartsBuff<ThundercrackBarrel, ThundercrackBarrelBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<MineralBuffMultiplierEvent>(OnModifyValueEvent);
		}

		private MineralBuffMultiplierEvent OnModifyValueEvent(MineralBuffMultiplierEvent e) {
			float multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
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
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					float multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
					int displayedMultiplier = (int) (multiplier * 100);

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("ThundercrackBarrel_desc", displayedMultiplier));
				})
			};
		}
	}
}