using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Attachments.ChainReaction {
	public class ChainReaction : WeaponPartsEntity<ChainReaction, ChainReactionBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "ChainReaction";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}


		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class ChainReactionBuff : WeaponPartsBuff<ChainReaction, ChainReactionBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			RegisterWeaponBuildBuffEvent<OnMineralAOEAddMalfunctionBuff>(OnMineralAOEAddMalfunctionBuff);
		}

		private void OnMineralAOEAddMalfunctionBuff(OnMineralAOEAddMalfunctionBuff e) {
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
			List<IBuff> buffs = buffSystem.GetBuffs(e.Target);
			
			foreach (var buff in buffs) {
				if(buff is MalfunctionBuff) {
					continue;
				}

				
				buff.RemainingDuration = buff.MaxDuration;
				if(buff is ILeveledBuff leveledBuff) {
					leveledBuff.LevelUp(1);
					buffSystem.SendBuffUpdateEvent(e.Target, weaponEntity, buff, BuffUpdateEventType.OnUpdate);
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
			UnRegisterWeaponBuildBuffEvent<OnMineralAOEAddMalfunctionBuff>(OnMineralAOEAddMalfunctionBuff);
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
						Localization.Get("ChainReaction_desc"));
				})
			};
		}
	}
}