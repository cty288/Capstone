using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Instances.AdrenalineSkill;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;

namespace _02._Scripts.Runtime.Skills.Model.Instance.AdrenalineSkill {
	public class AdrenalineSkill : SkillEntity<AdrenalineSkill> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "AdrenalineSkill";
		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string GetDescription(string defaultLocalizationKey) {
			int buffLevel = GetCustomPropertyOfCurrentLevel<int>("buff_level");
			
			StimulatedBuff buff = BuffPool.GetTemplateBuffs((buff => buff is StimulatedBuff)).FirstOrDefault() as StimulatedBuff;
			string buffDescription = buff.GetLevelDescription(buffLevel);

			return Localization.GetFormat(defaultLocalizationKey, buffLevel, buffDescription);
		}


		protected override void OnUpgrade(int previousLevel, int level) {
			
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
}