using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.Properties.SkillsBase;
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
			return GetProperty<ICustomProperties>().RealValues.Value;
		}

		public ICustomProperty GetCustomProperty(string key) {
			return GetProperty<ICustomProperties>().GetCustomProperty(key);
		}

		public ICustomDataProperty GetCustomDataProperty(string customPropertyName, string dataName) {
			return GetCustomProperty(customPropertyName)?.GetCustomDataProperty(dataName);
		}

		public ICustomDataProperty<T> GetCustomDataProperty<T>(string customPropertyName, string dataName) {
			return GetCustomProperty(customPropertyName)?.GetCustomDataProperty<T>(dataName);
		}

		public dynamic GetCustomDataValue(string customPropertyName, string dataName) {
			return GetCustomProperty(customPropertyName)?.GetCustomDataValue(dataName);
		}

		public T GetCustomDataValue<T>(string customPropertyName, string dataName) {
			if (!HasCustomProperty(customPropertyName)) {
				return default;
			}
			return GetCustomProperty(customPropertyName).GetCustomDataValue<T>(dataName);
		}

		public IUnRegister RegisterOnCustomDataChanged(string customPropertyName, string dataName, Action<ICustomDataProperty, object, object> onCustomDataChanged) {
			return GetCustomProperty(customPropertyName)?.RegisterOnCustomDataChanged(dataName, onCustomDataChanged);
		}

		public IUnRegister RegisterOnCustomDataChanged(string customPropertyName, Action<ICustomProperty> onCustomDataChanged) {
			return GetCustomProperty(customPropertyName)?.RegisterOnCustomDataChanged(onCustomDataChanged);
		}

		public void UnRegisterOnCustomDataChanged(string customPropertyName, string dataName, Action<ICustomDataProperty, object, object> onCustomDataChanged) {
			GetCustomProperty(customPropertyName)?.UnRegisterOnCustomDataChanged(dataName, onCustomDataChanged);
		}

		public void UnRegisterOnCustomDataChanged(string customPropertyName, Action<ICustomProperty> onCustomDataChanged) {
			GetCustomProperty(customPropertyName)?.UnRegisterOnCustomDataChanged(onCustomDataChanged);
		}

		public bool HasCustomProperty(string propertyName) {
			return GetProperty<ICustomProperties>().RealValues.Value.ContainsKey(propertyName);
		}
	}
}