using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Properties;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Skills.Model.Properties {
	public class PurchaseCostInfo : ICloneable{
		public Dictionary<string, int> ResourceCost;
		public int MoneyCost;
		
		public object Clone() {
			return new PurchaseCostInfo() {
				ResourceCost = new Dictionary<string, int>(ResourceCost),
				MoneyCost = MoneyCost
			};
		}
	}
	
	public interface IPurchaseCost : IProperty<PurchaseCostInfo>, ILoadFromConfigProperty {
		
	}
	
	
	public class PurchaseCost : AbstractLoadFromConfigProperty<PurchaseCostInfo>, IPurchaseCost {
		protected override PropertyName GetPropertyName() {
			return PropertyName.purchase_cost;
		}
		
		

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}

	}
	
}