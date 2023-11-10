﻿using _02._Scripts.Runtime.Skills.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;

namespace _02._Scripts.Runtime.Skills.Model.Instance {
	public class TestSkill : PassiveSkillEntity<TestSkill> {
		[field: ES3Serializable] public override string EntityName { get; set; } = "TestSkill";
		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override void OnInitModifiers(int rarity) {
		
		}

		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
}