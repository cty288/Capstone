using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Magazines.EMPChain {
	public class EMPChain : WeaponPartsEntity<EMPChain, EMPChainBuff> {
		public override string EntityName { get; set; } = "EMPChain";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float chance = GetCustomDataValueOfCurrentLevel<float>("chance");
			int displayChance = (int) (chance * 100);
			
			
			return Localization.GetFormat(defaultLocalizationKey, displayChance);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class EMPChainBuff : WeaponPartsBuff<EMPChain, EMPChainBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			RegisterWeaponBuildBuffEvent<PlantAOEEvent>(OnPlantAOEEvent);
		}

		private void OnPlantAOEEvent(PlantAOEEvent e) {
			float chance = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("chance");
			if(Random.Range(0f, 1f) < chance) {
				e.Buff.AddMulfunctionBuff(e.Duration, e.Target);
			}
		}
		

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		public override void OnRecycled() {
			UnRegisterWeaponBuildBuffEvent<PlantAOEEvent>(OnPlantAOEEvent);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					float chance = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("chance");
					int displayChance = (int) (chance * 100);

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("EMPChain_desc", displayChance));
				})
			};
		}
	}
}