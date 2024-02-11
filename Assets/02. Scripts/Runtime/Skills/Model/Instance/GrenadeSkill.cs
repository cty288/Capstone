using System.Collections.Generic;
using _02._Scripts.Runtime.Skills.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.Model.Instance {
	public class GrenadeSkill : SkillEntity<GrenadeSkill> {
		public override string EntityName { get; set; } = "GrenadeSkill";

		//public override string AnimLayerName { get; } = "GrenadeSkill";

		

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override void OnRegisterResourcePropertyDescriptionGetters(ref List<GetResourcePropertyDescriptionGetter> list) {
			base.OnRegisterResourcePropertyDescriptionGetters(ref list);

			list.Add(() => new ResourcePropertyDescription(null, Localization.Get(
				"PROPERTY_ICON_DAMAGE"), GetCustomPropertyOfCurrentLevel<int>("explosion_damage").ToString()));

			list.Add(() => new ResourcePropertyDescription(null, Localization.Get(
					"PROPERTY_ICON_RANGE"),
				Localization.GetFormat("PROPERTY_ICON_RANGE_DESC",
					Mathf.RoundToInt(GetCustomPropertyOfCurrentLevel<float>("explosion_radius")))));

		}

		public override bool Collectable { get; } = true;

		protected override string GetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
		
		
	}
}