using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.Skills.Model.Properties {
	public interface ISkillCostProperty : ILeveledProperty<Dictionary<CurrencyType, int>>, ILoadFromConfigProperty {
		int GetByLevel(int level, CurrencyType currencyType);
	}

	public interface ISkillUpgradeCost : ISkillCostProperty{
		
	}
	
	public abstract class SkillCostProperty : LeveledProperty<Dictionary<CurrencyType, int>>, ISkillCostProperty {
		public int GetByLevel(int level, CurrencyType currencyType) {
			Dictionary<CurrencyType, int> costByLevel = GetByLevel(level, RealValue.Value);
			if (costByLevel.TryGetValue(currencyType, out var cost)) {
				return cost;
			}
			return 0;
		}
	}
	
	public class SkillUpgradeCost : SkillCostProperty, ISkillUpgradeCost {
		protected override PropertyName GetPropertyName() {
			return PropertyName.skill_upgrade_cost;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}