using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Runtime.DataFramework.Properties.CustomProperties;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties {
	public interface IHaveCustomProperties : IEntity {

		public Dictionary<string, ICustomProperty> GetCustomProperties();

		/*public ICustomProperty GetCustomProperty(string key);
		

		public ICustomDataProperty GetCustomDataProperty(string customPropertyName, string dataName);

		public ICustomDataProperty<T> GetCustomDataProperty<T>(string customPropertyName, string dataName);
		*/

		public IBindableProperty GetCustomDataValue(string customPropertyName, string dataName);
		
		public BindableProperty<T> GetCustomDataValue<T>(string customPropertyName, string dataName);

		public IUnRegister RegisterOnCustomDataChanged(string customPropertyName, string dataName,
			Action<ICustomDataProperty, object, object> onCustomDataChanged);

		public IUnRegister RegisterOnCustomDataChanged(string customPropertyName, Action<ICustomProperty> onCustomDataChanged);

		public void UnRegisterOnCustomDataChanged(string customPropertyName, string dataName,
			Action<ICustomDataProperty, object, object> onCustomDataChanged);

		public void UnRegisterOnCustomDataChanged(string customPropertyName, Action<ICustomProperty> onCustomDataChanged);

		public bool HasCustomProperty(string propertyName);

	}
}