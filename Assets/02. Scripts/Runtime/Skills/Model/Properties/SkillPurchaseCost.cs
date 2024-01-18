using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Properties;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Skills.Model.Properties {
	public class SkillPurchaseCostInfo : ICloneable{
		public Dictionary<string, int> ResourceCost;
		public int MoneyCost;
		
		public object Clone() {
			return new SkillPurchaseCostInfo() {
				ResourceCost = new Dictionary<string, int>(ResourceCost),
				MoneyCost = MoneyCost
			};
		}
	}
	
	public interface ISkillPurchaseCost : IProperty<SkillPurchaseCostInfo>, ILoadFromConfigProperty {
		
	}
	
	
	public class SkillPurchaseCost : AbstractLoadFromConfigProperty<SkillPurchaseCostInfo>, ISkillPurchaseCost {
		protected override PropertyName GetPropertyName() {
			return PropertyName.skill_purchase_cost;
		}
		
		

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}

	}
	
}