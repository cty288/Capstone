using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Object = System.Object;

namespace Runtime.DataFramework.Properties.CustomProperties {
	public interface ICustomDataProperty : IPropertyBase {
		public string CustomDataName { get; }
		public dynamic OnGetBaseValueFromConfig(dynamic value);
	}
	
	public interface ICustomDataProperty<T> : ICustomDataProperty, IProperty<T> {
		
		dynamic ICustomDataProperty.OnGetBaseValueFromConfig(dynamic value) {
			return OnGetBaseValueFromConfig(value);
		}

		public T OnGetBaseValueFromConfig(dynamic value);
		
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

		public T OnGetBaseValueFromConfig(dynamic value) {
			if (typeof(T) == typeof(object)) {
				if (value is JToken token) {
					return (T) ConvertJTokenToBaseValue(token);
				}
			}
			return JsonConvert.DeserializeObject<T>(value.ToString());
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