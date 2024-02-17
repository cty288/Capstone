using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Combat.Attachments.Multivirus {
	public class Multivirus : WeaponPartsEntity<Multivirus, MultivirusBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "Multivirus";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int times = GetCustomDataValueOfCurrentLevel<int>("number");
			return Localization.GetFormat(defaultLocalizationKey, times);
		}

		public override int GetMaxRarity() {
			return 3;
		}


		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class MultivirusBuff : WeaponPartsBuff<Multivirus, MultivirusBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			RegisterWeaponBuildBuffEvent<OnHackedBuffAdded>(OnHackedBuffAdded);
		}

		private void OnHackedBuffAdded(OnHackedBuffAdded e) {
			List<BuffBuilder> buffBuilders = BuffPool.FindBuffs((buff => !buff.IsGoodBuff && buff is not HackedBuff));
			int number = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("number");
			number = Mathf.Min(number, buffBuilders.Count);
			
			buffBuilders.Shuffle();
			
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
			for (int i = 0; i < number; i++) {
				BuffBuilder buffBuilder = buffBuilders[i];
				IBuff buff = buffBuilder.Invoke(e.WeaponEntity, e.Target, 1);
				if (!buffSystem.AddBuff(e.Target, e.WeaponEntity, buff)) {
					buff.RecycleToCache();
				}
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
			UnRegisterWeaponBuildBuffEvent<OnHackedBuffAdded>(OnHackedBuffAdded);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int times = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("number");
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("Multivirus_desc", times));
				})
			};
		}
	}
}