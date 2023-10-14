using System;
using System.Collections.Generic;
using System.Dynamic;
using _02._Scripts.Runtime.Levels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.ConfigSheet;
using Object = System.Object;

namespace Runtime.DataFramework.Properties.CustomProperties {
	public interface ICustomDataProperty : IPropertyBase {
		public string CustomDataName { get; }
		public dynamic OnGetBaseValueFromConfig(dynamic value, IEntity parentEntity);
	}
	
	public interface ICustomDataProperty<T> : ICustomDataProperty, IProperty<T> {
		
		dynamic ICustomDataProperty.OnGetBaseValueFromConfig(dynamic value, IEntity parentEntity) {
			return OnGetBaseValueFromConfig(value, parentEntity);
		}

		public T OnGetBaseValueFromConfig(dynamic value, IEntity parentEntity);
		
		//a == b
		
	}
	
	public class CustomDataProperty<T> : Property<T>, ICustomDataProperty, ICustomDataProperty<T> {
		[field: ES3Serializable]
		public string CustomDataName { get; private set; }

		[field: ES3Serializable]
		protected PropertyNameInfo[] defaultDependencies;
		
		public CustomDataProperty(): base(){}
		
		public CustomDataProperty(string customDataName, IPropertyDependencyModifier<T> modifier = null, PropertyNameInfo[] defaultDependencies = null) : base() {
			CustomDataName = customDataName;
			SetModifier(modifier);
			this.defaultDependencies = defaultDependencies;
		}

		protected dynamic ConvertJTokenToBaseValue(JToken token)
		{
			if (token is JValue v)
			{
				if (v.Value is long)
				{
					return Convert.ToInt32(v.Value);
				}

				if (v.Value is double)
				{
					return Convert.ToSingle(v.Value);
				}
				return v.Value;
			}
			else if (token is JObject o)
			{
				var result = new ExpandoObject() as IDictionary<string, Object>;
				foreach (var property in o.Properties())
				{
					result[property.Name] = ConvertJTokenToBaseValue(property.Value);
				}
				return result;
			}
			else if (token is JArray a)
			{
				var result = new List<object>();
				foreach (var item in a.Children())
				{
					result.Add(ConvertJTokenToBaseValue(item));
				}
				return result;
			}

			throw new ArgumentException($"Unsupported token type: {token.GetType().Name}");
		}

		public T OnGetBaseValueFromConfig(dynamic value, IEntity parentEntity) {
			string rawVal = value["value"].ToString();
			
			try {
				if (value["dependencies"] is JArray dependencies) {
						
					defaultDependencies = new PropertyNameInfo[dependencies.Count];
					for (int i = 0; i < dependencies.Count; i++) {
						defaultDependencies[i] = new PropertyNameInfo(dependencies[i].ToString());
					}
				}
				
				if(value["useDefaultModifier"] is JValue {Value: bool and true}){
					//if rarity and level are not present in default dependencies, add them
					if (defaultDependencies == null) {
						defaultDependencies = new PropertyNameInfo[2];
						defaultDependencies[0] = new PropertyNameInfo(PropertyName.rarity);
						defaultDependencies[1] = new PropertyNameInfo(PropertyName.level_number);
					}
					else {
						bool hasRarity = false;
						bool hasLevel = false;
						foreach (var defaultDependency in defaultDependencies) {
							if (defaultDependency.GetFullName() == "rarity") {
								hasRarity = true;
							}
							else if (defaultDependency.GetFullName() == "level_number") {
								hasLevel = true;
							}
						}

						if (!hasRarity) {
							var newDefaultDependencies = new PropertyNameInfo[defaultDependencies.Length + 1];
							for (int i = 0; i < defaultDependencies.Length; i++) {
								newDefaultDependencies[i] = defaultDependencies[i];
							}

							newDefaultDependencies[defaultDependencies.Length] = new PropertyNameInfo(PropertyName.rarity);
							defaultDependencies = newDefaultDependencies;
						}
							
						if (!hasLevel) {
							var newDefaultDependencies = new PropertyNameInfo[defaultDependencies.Length + 1];
							for (int i = 0; i < defaultDependencies.Length; i++) {
								newDefaultDependencies[i] = defaultDependencies[i];
							}

							newDefaultDependencies[defaultDependencies.Length] = new PropertyNameInfo(PropertyName.level_number);
							defaultDependencies = newDefaultDependencies;
						}
					}
						
					SetModifier<T>(GlobalLevelFormulas.GetGeneralEnemyAbilityModifier<T>(
						() => parentEntity.GetProperty<IRarityProperty>().BaseValue,
						() => parentEntity.GetProperty<ILevelNumberProperty>().BaseValue));
				}
			}
			catch (Exception e) {
					
			}
			
			
			if (typeof(T) == typeof(object)) {
				string type = value["type"];
				Type parsedType = SerializationFactory.Singleton.ParseType(type);
				
				
				dynamic result = null;
				if (parsedType == typeof(string)) {
					result = rawVal;
				}
				else if (parsedType == typeof(object))  {
					result = (T) ConvertJTokenToBaseValue(value["value"]);
				}
				else {
					result = JsonConvert.DeserializeObject(rawVal, parsedType);
				}

				
				return result;
			}
			return JsonConvert.DeserializeObject<T>(rawVal);
		}

		
		protected override IPropertyDependencyModifier<T> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.custom_property_data;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return defaultDependencies;
		}

		
	}



}