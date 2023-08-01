using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.Properties.SkillsBase;
using MikroFramework.BindableProperty;
using MikroFramework.Event;

namespace _02._Scripts.Runtime.Base.Entity.ClassifiedEntity {
	public abstract class AbstractHaveCustomPropertiesEntity: global::Entity, IHaveCustomProperties {
		
		
		protected override void OnRegisterProperties() {
			ICustomProperty[] properties = OnRegisterCustomProperties();
			RegisterInitialProperty<ICustomProperties>(new CustomProperties(properties));
			OnEntityRegisterProperties();
		}

		protected abstract void OnEntityRegisterProperties();
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
		
		
		//TODO: add set
	}
}