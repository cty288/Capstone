using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.Skills.Model.Instance.MineralRobotSkill {
	public class MineralRobotSkill : SkillEntity<MineralRobotSkill> {
		[ES3Serializable]
		private int spawnedCount = 0;
		
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "MineralRobotSkill";
		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable { get; } = true;
		protected override string GetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override void OnUpgrade(int previousLevel, int level) {
			
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}

		public override void OnRecycle() {
			base.OnRecycle();
			spawnedCount = 0;
		}
		protected override bool UseCurrencySatisfiedCondition(Dictionary<CurrencyType, int> currency) {
			return base.UseCurrencySatisfiedCondition(currency) && spawnedCount == 0;
		}
		public override void OnRegisterResourcePropertyDescriptionGetters(ref List<GetResourcePropertyDescriptionGetter> list) {
			base.OnRegisterResourcePropertyDescriptionGetters(ref list);
			
			list.Add(() => {
				int limit = GetCustomPropertyOfCurrentLevel<int>("limit");
				return new ResourcePropertyDescription(null, Localization.Get(
					"MineralRobotSkill_PROPERTY_TOTAL"), limit.ToString());
			});
			list.Add(() => {
				float interval = GetCustomPropertyOfCurrentLevel<float>("interval");
				int count = GetCustomPropertyOfCurrentLevel<int>("count");
				return new ResourcePropertyDescription(null, Localization.Get(
						"MineralRobotSkill_PROPERTY_EFFICIENCY"),
					Localization.GetFormat("MineralRobotSkill_PROPERTY_EFFICIENCY_VALUE", interval, count));
			});
		}

		public void OnSpawnRobot(MineralRobotEntity entity) {
			if(entity == null) return;
			
			spawnedCount++;
			entity.RegisterOnEntityRecycled(OnRobotRecycled);
		}

		private void OnRobotRecycled(IEntity entity) {
			entity.UnRegisterOnEntityRecycled(OnRobotRecycled);
			spawnedCount--;
		}
	}
}