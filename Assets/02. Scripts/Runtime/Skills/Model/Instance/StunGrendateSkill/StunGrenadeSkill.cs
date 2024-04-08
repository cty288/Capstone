using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.Skills.Model.Instance.StunGrendateSkill {
	public class StunGrenadeSkill: SkillEntity<StunGrenadeSkill>  {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "StunGrenadeSkill";
		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;

		public override void OnRegisterResourcePropertyDescriptionGetters(ref List<GetResourcePropertyDescriptionGetter> list) {
			base.OnRegisterResourcePropertyDescriptionGetters(ref list);
			
			list.Add(() => {
				float range = GetCustomPropertyOfCurrentLevel<float>("range");
				return new ResourcePropertyDescription(null, Localization.Get(
					"StunGrenadeSkill_RANGE"), Localization.GetFormat("StunGrenadeSkill_RANGE_VALUE", range));
			});
			
			
			list.Add(() => {
				MalfunctionBuff malfunctionBuff = BuffPool.GetTemplateBuff<MalfunctionBuff>();
				float buff1Time = GetCustomPropertyOfCurrentLevel<float>("malfunction_time");
				return new ResourcePropertyDescription(null, Localization.GetFormat(
						"StunGrenadeSkill_BUFF1_TIME", malfunctionBuff.GetDisplayName()),
					Localization.GetFormat("StunGrenadeSkill_BUFF_TIME_VALUE", buff1Time));
			});

			
				
			list.Add(() => {
				if (GetLevel() >= 3) {
					float buff2Time = GetCustomPropertyOfCurrentLevel<float>("powerless_time");
					PowerlessBuff powerlessBuff = BuffPool.GetTemplateBuff<PowerlessBuff>();
					
					return new ResourcePropertyDescription(null, Localization.GetFormat(
							"StunGrenadeSkill_BUFF2_TIME", powerlessBuff.GetDisplayName(-1)),
						Localization.GetFormat("StunGrenadeSkill_BUFF_TIME_VALUE", buff2Time));
				}
				else {
					return new ResourcePropertyDescription(null, null, null, false);
				}
				
			});
		
			
		}

		protected override string GetDescription(string defaultLocalizationKey) {
			MalfunctionBuff malfunctionBuff = BuffPool.GetTemplateBuffs((buff => buff is MalfunctionBuff)).FirstOrDefault() as MalfunctionBuff;
			int powerlessBuffLevel = GetCustomPropertyOfCurrentLevel<int>("powerless_level");
			PowerlessBuff powerlessBuff = BuffPool.GetTemplateBuff<PowerlessBuff>();
			string displayedPowerlessName = powerlessBuff.GetDisplayName(powerlessBuffLevel);
			
			string key = defaultLocalizationKey;
			string powerLessBuffDesc = "";

			string malfunctionBuffDescription =
				$"<b>{malfunctionBuff.GetDisplayName()}: </b>{malfunctionBuff.GetDescription()}";
			
			if (GetLevel() >= 3) {
				key = "StunGrenadeSkill_desc2";
				
				
				powerLessBuffDesc += "\n\n" + powerlessBuff.GetLevelDescription(powerlessBuffLevel);
			}
			else {
				displayedPowerlessName = "";
			}

			return Localization.GetFormat(key,
				malfunctionBuffDescription, powerLessBuffDesc, displayedPowerlessName,
				malfunctionBuff.GetDisplayName());
			
		}

		protected override void OnAddedToHotBar() {
			
		}

		protected override void OnRemovedFromHotBar() {
			
		}

		protected override void OnUpgrade(int previousLevel, int level) {
			
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
}