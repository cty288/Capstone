using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.Properties.TagProperty;
using Runtime.Enemies.Model.Properties;

namespace Runtime.DataFramework.Entities {
	
	/// <summary>
	/// Basically all entities inherit from this class. Auto register the custom properties, rarity, and tags
	/// </summary>
	public abstract class AbstractBasicEntity: global::Runtime.DataFramework.Entities.Entity, IHaveCustomProperties, IHaveTags {
		
		
		protected override void OnRegisterProperties() {
			ICustomProperty[] properties = OnRegisterCustomProperties();
			RegisterInitialProperty(new Rarity());
			RegisterInitialProperty<ITagProperty>(new TagProperty());
			RegisterInitialProperty<ICustomProperties>(new Properties.CustomProperties.CustomProperties(properties));
			OnEntityRegisterAdditionalProperties();
		}

		/// <summary>
		/// A place to register additional properties, normally empty. Recommended to register custom properties instead.
		/// </summary>
		protected abstract void OnEntityRegisterAdditionalProperties();
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected abstract ICustomProperty[] OnRegisterCustomProperties();
		

		public Dictionary<string, ICustomProperty> GetCustomProperties() {
			return GetProperty<ICustomProperties>().RealValues?.Value;
		}

		private ICustomProperty GetCustomProperty(string key) {
			return GetProperty<ICustomProperties>().GetCustomProperty(key);
		}

		private ICustomDataProperty GetCustomDataProperty(string customPropertyName, string dataName) {
			return GetCustomProperty(customPropertyName)?.GetCustomDataProperty(dataName);
		}

		private ICustomDataProperty<T> GetCustomDataProperty<T>(string customPropertyName, string dataName) {
			return GetCustomProperty(customPropertyName)?.GetCustomDataProperty<T>(dataName);
		}

		public IBindableProperty GetCustomDataValue(string customPropertyName, string dataName) {
			return GetCustomProperty(customPropertyName)?.GetCustomDataValue(dataName);
		}

		/// <summary>
		/// If your custom property is DataOnlyProperty, use dynamic for T
		/// </summary>
		/// <param name="customPropertyName"></param>
		/// <param name="dataName"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public BindableProperty<T> GetCustomDataValue<T>(string customPropertyName, string dataName) {
			if (!HasCustomProperty(customPropertyName)) {
				return default;
			}
			return GetCustomProperty(customPropertyName).GetCustomDataValue<T>(dataName);
		}

		public IUnRegister RegisterOnCustomDataChanged(string customPropertyName, string dataName, Action<ICustomDataProperty, dynamic, dynamic> onCustomDataChanged) {
			return GetCustomProperty(customPropertyName)?.RegisterOnCustomDataChanged(dataName, onCustomDataChanged);
		}

		public IUnRegister RegisterOnCustomDataChanged(string customPropertyName, Action<ICustomProperty> onCustomDataChanged) {
			return GetCustomProperty(customPropertyName)?.RegisterOnCustomDataChanged(onCustomDataChanged);
		}

		public void UnRegisterOnCustomDataChanged(string customPropertyName, string dataName, Action<ICustomDataProperty, dynamic, dynamic> onCustomDataChanged) {
			GetCustomProperty(customPropertyName)?.UnRegisterOnCustomDataChanged(dataName, onCustomDataChanged);
		}

		public void UnRegisterOnCustomDataChanged(string customPropertyName, Action<ICustomProperty> onCustomDataChanged) {
			GetCustomProperty(customPropertyName)?.UnRegisterOnCustomDataChanged(onCustomDataChanged);
		}

		public bool HasCustomProperty(string propertyName) {
			return GetProperty<ICustomProperties>().RealValues.Value.ContainsKey(propertyName);
		}
		
		
		public ITagProperty GetTagProperty() {
			return GetProperty<ITagProperty>();
		}
	}
}