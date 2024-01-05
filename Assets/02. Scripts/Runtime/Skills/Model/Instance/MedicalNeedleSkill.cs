using System.Collections.Generic;
using _02._Scripts.Runtime.Skills.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;

namespace _02._Scripts.Runtime.Skills.Model.Instance {
	public class MedicalNeedleSkill : SkillEntity<MedicalNeedleSkill> {
		public override string EntityName { get; set; } = "MedicalNeedleSkill";
		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override string GetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
		
		public override void OnRegisterResourcePropertyDescriptionGetters(ref List<GetResourcePropertyDescriptionGetter> list) {
			base.OnRegisterResourcePropertyDescriptionGetters(ref list);

			list.Add(() => new ResourcePropertyDescription("PropertyIconHeal", Localization.GetFormat(
				                                                                   "PROPERTY_NEEDLE_HEAL",
				                                                                   GetCustomPropertyOfCurrentLevel<int>(
					                                                                   "healing_amount"))
			                                                                   + (GetLevel() >= 3
				                                                                   ? Localization.GetFormat(
					                                                                   "PROPERTY_NEEDLE_HEAL_BUFF",
					                                                                   GetCustomPropertyOfCurrentLevel<
						                                                                   int>("buff_effect"), 1,
					                                                                   GetCustomPropertyOfCurrentLevel<
						                                                                   float>("buff_duration"))
				                                                                   : ""), true));

		}
	}
}