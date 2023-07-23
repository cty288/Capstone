using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Base.Property;
using _02._Scripts.Runtime.Common.Properties.CustomsBase;
using MikroFramework.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.Properties {
	
	public interface ICustomProperties : IDictionaryProperty<string, ICustomProperty> {
		public ICustomProperty GetCustomProperty(string key);
		
	}
	
	/// <summary>
	/// Structure: CustomsProperty -> CustomProperty[] -> CustomDataProperty[]
	/// </summary>

	public class CustomProperties :  PropertyDictionary<string,ICustomProperty>, ICustomProperties {
		
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
		

		public override Dictionary<string, ICustomProperty> OnSetBaseValueFromConfig(dynamic value) {
			//get all keys of value
			_configData = value;
			IEnumerable<string> keys = _configData.Properties().Select(p => p.Name);
			foreach (string key in keys) {
				if (BaseValue.TryGetValue(key, out ICustomProperty val)) {
					val.LoadFromConfig(value[key]);
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

		public override string GetKey(ICustomProperty value) {
			return value.CustomPropertyName;
		}

		public void AddCustomToRealValue(ICustomProperty custom, IEntity parentEntity, bool loadFromConfig) {
			if (loadFromConfig) {
				if (_configData == null) {
					Debug.LogError("Config data not set!");
					return;
				}
				if (!_configData.ContainsKey(custom.CustomPropertyName)) {
					Debug.LogError("Custom " + custom.CustomPropertyName + " not found in config!");
					return;
				}
				custom.LoadFromConfig(_configData[custom.CustomPropertyName]);
			}

			AddToRealValue(custom, parentEntity);
		}

		public void RemoveCustomFromRealValue(string key) {
			RemoveFromRealValue(key);
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