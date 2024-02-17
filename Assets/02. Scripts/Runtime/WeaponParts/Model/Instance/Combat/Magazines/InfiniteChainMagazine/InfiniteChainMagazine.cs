using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Mineral;
using Cysharp.Threading.Tasks;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Magazines.InfiniteChainMagazine {
	public class InfiniteChainMagazine : WeaponPartsEntity<InfiniteChainMagazine, InfiniteChainMagazineBuff> {
		public override string EntityName { get; set; } = "InfiniteChainMagazine";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int number = GetCustomDataValueOfCurrentLevel<int>("number");
			return Localization.GetFormat(defaultLocalizationKey, number);
		}

		public override int GetMaxRarity() {
			return 3;
		}

		public override int GetMinRarity() {
			return 2;
		}


		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class InfiniteChainMagazineBuff : WeaponPartsBuff<InfiniteChainMagazine, InfiniteChainMagazineBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
	
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<CombatBuffOnModifyBaseAmmoRecoverEvent>(OnModifyBaseAmmoRecover);
		}

		private CombatBuffOnModifyBaseAmmoRecoverEvent OnModifyBaseAmmoRecover(CombatBuffOnModifyBaseAmmoRecoverEvent e) {
			int number = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("number");
			e.Value += number;
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
			weaponEntity.UnRegisterOnModifyValueEvent<CombatBuffOnModifyBaseAmmoRecoverEvent>(OnModifyBaseAmmoRecover);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int number = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("number");

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("InfiniteChainMagazine_desc", number));
				})
			};
		}
	}
}