using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.Skills.Model.Properties {
	public interface ISkillUseCost : ISkillCostProperty{
		
	}
	
	
	public class SkillUseCost : SkillCostProperty, ISkillUseCost {
		protected override PropertyName GetPropertyName() {
			return PropertyName.skill_use_cost;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}