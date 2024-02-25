using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Combat.Barrels.DawnbreakerBlast {
	public class DawnbreakerBlast : WeaponPartsEntity<DawnbreakerBlast, DawnbreakerBlastBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "DawnbreakerBlast";
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
	
	public class DawnbreakerBlastBuff : WeaponPartsBuff<DawnbreakerBlast, DawnbreakerBlastBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<OnCombatBuffModifyExplosionDOR>(OnCombatBuffModifyExplosionDOR);
		}

		private OnCombatBuffModifyExplosionDOR OnCombatBuffModifyExplosionDOR(OnCombatBuffModifyExplosionDOR e) {
			int damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
			e.Value = damage;
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
			weaponEntity.UnRegisterOnModifyValueEvent<OnCombatBuffModifyExplosionDOR>(OnCombatBuffModifyExplosionDOR);
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
					int calculatedDamage = damage * weaponEntity.GetRarity();
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("DawnbreakerBlast_PROPERTY_desc", damage, calculatedDamage));
				})
			};
		}
	}
}