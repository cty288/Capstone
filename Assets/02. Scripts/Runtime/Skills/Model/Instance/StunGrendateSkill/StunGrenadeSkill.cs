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
			float range = GetCustomPropertyOfCurrentLevel<float>("range");
			list.Add(() => new ResourcePropertyDescription(null, Localization.Get(
				"StunGrenadeSkill_RANGE"), Localization.GetFormat("StunGrenadeSkill_RANGE_VALUE", range)));
			
			float buff1Time = GetCustomPropertyOfCurrentLevel<float>("malfunction_time");
			list.Add(() => new ResourcePropertyDescription(null, Localization.Get(
				"StunGrenadeSkill_BUFF1_TIME"), Localization.GetFormat("StunGrenadeSkill_BUFF_TIME_VALUE", buff1Time)));

			if (GetLevel() >= 3) {
				float buff2Time = GetCustomPropertyOfCurrentLevel<float>("powerless_time");
				list.Add(() => new ResourcePropertyDescription(null, Localization.Get(
						"StunGrenadeSkill_BUFF2_TIME"),
					Localization.GetFormat("StunGrenadeSkill_BUFF_TIME_VALUE", buff2Time)));
			}
			
		}

		protected override string GetDescription(string defaultLocalizationKey) {
			MalfunctionBuff malfunctionBuff = BuffPool.GetTemplateBuffs((buff => buff is MalfunctionBuff)).FirstOrDefault() as MalfunctionBuff;
			int powerlessBuffLevel = GetCustomPropertyOfCurrentLevel<int>("powerless_level");
			string displayedPowerlessLevel = powerlessBuffLevel.ToString();
			
			string key = defaultLocalizationKey;
			string powerLessBuffDesc = "";
			
			if (GetLevel() >= 3) {
				key = "StunGrenadeSkill_desc2";
				PowerlessBuff powerlessBuff =
					BuffPool.GetTemplateBuffs((buff => buff is PowerlessBuff)).FirstOrDefault() as PowerlessBuff;
				
				powerLessBuffDesc += "\n\n" + powerlessBuff.GetLevelDescription(powerlessBuffLevel);
			}
			else {
				displayedPowerlessLevel = "";
			}

			return Localization.GetFormat(key,
				malfunctionBuff.OnGetDescription("MulfunctionBuff_Desc"), powerLessBuffDesc, displayedPowerlessLevel);
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
}