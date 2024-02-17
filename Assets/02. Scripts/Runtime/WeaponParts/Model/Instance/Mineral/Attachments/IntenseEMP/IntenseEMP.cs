using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Attachments {
	public class IntenseEMP : WeaponPartsEntity<IntenseEMP, IntenseEMPBuff> {
		public override string EntityName { get; set; } = "IntenseEMP";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplier = GetCustomDataValueOfCurrentLevel<float>("multiplier");
			int displayMultiplier = (int) (multiplier * 100);

			if (GetRarity() <= 2) {
				return Localization.GetFormat(defaultLocalizationKey, displayMultiplier);
			}else {
				return Localization.GetFormat("IntenseEMP_desc_2", displayMultiplier);	
			}
			
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class IntenseEMPBuff : WeaponPartsBuff<IntenseEMP, IntenseEMPBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			RegisterWeaponBuildBuffEvent<MineralAOEKillEvent>(OnPlantAOEKillEvent);
		}

		private void OnPlantAOEKillEvent(MineralAOEKillEvent e) {
			float range = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("range");
			float multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
			var weaponDamage = weaponEntity.GetBaseDamage().RealValue.Value;
			int damage = Random.Range(weaponDamage.x, weaponDamage.y);
			
			int realDamage = Mathf.RoundToInt(damage * multiplier);

			e.Buff2.RangeAOE(range, realDamage, 1, e.Transform, e.Target, weaponEntity, false);
		}

		private void OnPlantAOEEvent(MineralAOEEvent e) {
			float chance = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("chance");
			if(Random.Range(0f, 1f) < chance) {
				e.Buff2.AddMulfunctionBuff(e.Duration, e.Target);
			}
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
			UnRegisterWeaponBuildBuffEvent<MineralAOEKillEvent>(OnPlantAOEKillEvent);
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
					int displayMultiplier = (int) (multiplier * 100);
					string defaultLocalizationKey = "IntenseEMP_desc";
					if (weaponPartsEntity.GetRarity() > 2) {
						defaultLocalizationKey = "IntenseEMP_desc_2";
					}
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat(defaultLocalizationKey, displayMultiplier));
				})
			};
		}
	}
}