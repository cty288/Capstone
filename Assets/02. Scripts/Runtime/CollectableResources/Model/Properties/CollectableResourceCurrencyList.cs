using System;
using _02._Scripts.Runtime.Currency.Model;
using Runtime.DataFramework.Properties;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

//using PropertyName = UnityEngine.PropertyName;

namespace _02._Scripts.Runtime.CollectableResources.Model.Properties {

	[Serializable]
	public class CollectableResourceCurrencyInfo : ICloneable{
		public CurrencyType currencyType;
		public Vector2Int amountRange;


		public object Clone() {
			return new CollectableResourceCurrencyInfo {
				currencyType = currencyType,
				amountRange = amountRange
			};
		}
	}
	
	public interface ICollectableResourceCurrencyList : IListProperty<CollectableResourceCurrencyInfo> {
		
	}

	public class CollectableResourceCurrencyList : ListProperty<CollectableResourceCurrencyInfo>,
		ICollectableResourceCurrencyList{
		protected override PropertyName GetPropertyName() {
			return PropertyName.collectable_resource_currency_list;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity), new PropertyNameInfo(PropertyName.level_number)};
		}

	}
}