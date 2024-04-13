using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;

namespace _02._Scripts.Runtime.Skills.Model.Instance {
	public class TestSkill : SkillEntity<TestSkill> {
		public override bool Collectable { get; } = false;
		[field: ES3Serializable] public override string EntityName { get; set; } = "TestSkill";
		

	

		protected override string GetDescription(string defaultLocalizationKey) {
			return null;
		}

		protected override void OnAddedToHotBar() {
			
		}

		protected override void OnRemovedFromHotBar() {
			
		}

		protected override void OnInitModifiers(int rarity) {
		
		}


		protected override void OnUpgrade(int previousLevel, int level) {
			
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
}