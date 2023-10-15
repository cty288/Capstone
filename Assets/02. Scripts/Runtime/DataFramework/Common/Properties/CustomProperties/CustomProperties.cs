using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Runtime.DataFramework.Entities;
using UnityEngine;

namespace Runtime.DataFramework.Properties.CustomProperties {
	
	public interface ICustomProperties : IPropertyDictionary<string, ICustomProperty>, ILoadFromConfigProperty {
		public ICustomProperty GetCustomProperty(string key);
		
	}
	
	/// <summary>
	/// Structure: CustomsProperty -> CustomProperty[] -> CustomDataProperty[]
	/// </summary>

	public class CustomProperties :  PropertyDictionaryLoadFromConfig<string,ICustomProperty>, ICustomProperties {
		
		private JObject _configData;
		
		/// <summary>
		/// Please use CustomsProperty(params ICustomProperty[] Customs) to register all Customs at once
		/// </summary>
		public CustomProperties() : base() {
		}
		
		public CustomProperties(params ICustomProperty[] Customs) : base() {
			if (Customs != null) {
				BaseValue = Customs.ToDictionary(GetKey);
			}
			
		}
		

		public override Dictionary<string, ICustomProperty> OnSetBaseValueFromConfig(dynamic value, IEntity parentEntity) {
			//get all keys of value
			_configData = value;
			IEnumerable<string> keys = _configData.Properties().Select(p => p.Name);
			if (BaseValue == null) {
				return null;
			}
			foreach (string key in keys) {
				if (BaseValue.TryGetValue(key, out ICustomProperty val)) {
					val.SetBaseValue(val.OnGetBaseValueFromConfig(value[key], parentEntity));
				}
			}

			return BaseValue;
		}

		protected override IPropertyDependencyModifier<Dictionary<string, ICustomProperty>> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.custom_properties;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}

		public override string GetKey(ICustomProperty value) {
			return value.GetCustomPropertyName();
		}
		

		public ICustomProperty GetCustomProperty(string key) {
			if (RealValues.Value.TryGetValue(key, out ICustomProperty Custom)) {
				return Custom;
			}
			else {
				Debug.LogError("Custom " + key + " not found!");
				return null;
			}
		}
	}
}