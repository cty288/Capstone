using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.Combat.Attachments.DangerousModification;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Attachments.DangerousModification_EMP {
	
	public class DangerousModification_EMP : WeaponPartsEntity<DangerousModification_EMP, DangerousModification_EMPBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "DangerousModification_EMP";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float chance = GetCustomDataValueOfCurrentLevel<float>("chance");
			int displayChance = (int) (chance * 100);

			float time = GetCustomDataValueOfCurrentLevel<float>("time");

			return Localization.GetFormat(defaultLocalizationKey, displayChance, time);
		}

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class DangerousModification_EMPBuff : WeaponPartsBuff<DangerousModification_EMP, DangerousModification_EMPBuff> {
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = 0.2f;
		
		[ES3Serializable]
		private float lockRemainingTime = 0;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<MineralBuffModifyChanceEvent>(OnModifyChance);
			RegisterWeaponBuildBuffEvent<OnMineralAOEGenerateOriginalMulfunctionBuff>(OnMineralAOEGenerateOriginalMulfunctionBuff);
		}

		private MineralBuffModifyChanceEvent OnModifyChance(MineralBuffModifyChanceEvent e) {
			e.Value *= 2;
			return e;
		}

		private void OnMineralAOEGenerateOriginalMulfunctionBuff(OnMineralAOEGenerateOriginalMulfunctionBuff e) {
			float chance = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("chance");
			if (Random.Range(0f, 1f) < chance) {
				if (lockRemainingTime <= 0) {
					weaponEntity.LockWeaponCounter.Retain();
				}

				lockRemainingTime = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("time");
			}
		}




		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			float before = lockRemainingTime;
			lockRemainingTime -= TickInterval;
			if (before > 0 && lockRemainingTime <= 0) {
				weaponEntity.LockWeaponCounter.Release();
			}
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override void OnRecycled() {
			if (lockRemainingTime > 0) {
				weaponEntity.LockWeaponCounter.Release();
			}
			UnRegisterWeaponBuildBuffEvent<OnMineralAOEGenerateOriginalMulfunctionBuff>(OnMineralAOEGenerateOriginalMulfunctionBuff);
			weaponEntity.UnRegisterOnModifyValueEvent<MineralBuffModifyChanceEvent>(OnModifyChance);
			base.OnRecycled();
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					float chance = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("chance");
					int displayChance = (int) (chance * 100);
					
					float time = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("time");
					
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("DangerousModification_EMP_desc", displayChance, time));
				})
			};
		}
	}
}