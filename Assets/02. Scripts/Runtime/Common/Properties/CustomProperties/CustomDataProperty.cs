using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _02._Scripts.Runtime.Common.Properties.SkillsBase {
	public interface ICustomDataProperty : IPropertyBase {
		public string CustomDataName { get; }
	}
	
	public interface ICustomDataProperty<T> : ICustomDataProperty, IProperty<T> {
		
	}
	
	public class CustomDataProperty<T> : Property<T>, ICustomDataProperty, ICustomDataProperty<T> {
		[field: ES3Serializable]
		public string CustomDataName { get; private set; }

		[field: ES3Serializable]
		protected PropertyName[] dependencies;
		
		public CustomDataProperty(string customDataName, IPropertyDependencyModifier<T> modifier = null, 
			params PropertyName[] dependencies) : base() {
			CustomDataName = customDataName;
			SetModifier(modifier);
			this.dependencies = dependencies;
			
		}

		public override T OnSetBaseValueFromConfig(dynamic value) {
			if (value is JValue v) {
				return (T) v.Value;
			}

			if (typeof(T) == typeof(object)) {
				//return JsonConvert.DeserializeObject<dynamic>(value.ToString());
				return value;
			}
			
			return JsonConvert.DeserializeObject<T>(value.ToString());
		}

		protected override IPropertyDependencyModifier<T> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.custom_property_data;
		}

		public override PropertyName[] GetDependentProperties() {
			return dependencies;
		}

		
	}



}