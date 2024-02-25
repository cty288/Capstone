using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Magazines.FunctionalLimit {
	public class FunctionalLimit : WeaponPartsEntity<FunctionalLimit, FunctionalLimitBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "FunctionalLimit";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int buffLevel = GetCustomDataValueOfCurrentLevel<int>("buff_level");
			return Localization.GetFormat(defaultLocalizationKey, buffLevel);
		}

		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class FunctionalLimitBuff : WeaponPartsBuff<FunctionalLimit, FunctionalLimitBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			RegisterWeaponBuildBuffEvent<OnHackedBuffAdded>(OnHackedBuffAdded);
		}

		private void OnHackedBuffAdded(OnHackedBuffAdded e) {
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
			BuffBuilder buffBuilder = BuffPool.FindBuffs((buff => buff is PowerlessBuff)).FirstOrDefault();
			int buffLevel = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("buff_level");
			IBuff buff = buffBuilder?.Invoke(e.WeaponEntity, e.Target, buffLevel);
			
			if (!buffSystem.AddBuff(e.Target, e.WeaponEntity, buff)) {
				buff?.RecycleToCache();
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
					int buffLevel = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("buff_level");
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("FunctionalLimit_desc", buffLevel));
				})
			};
		}
	}
}