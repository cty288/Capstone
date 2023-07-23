using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.Properties.CustomsBase;
using _02._Scripts.Runtime.Common.Properties.CustomsBase;
using _02._Scripts.Runtime.Common.Properties.SkillsBase;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.Event;

namespace _02._Scripts.Runtime.Base.Entity.ClassifiedEntity {
	public interface IHaveCustomProperties : IEntity {

		public Dictionary<string, ICustomProperty> GetCustomProperties();

		public ICustomProperty GetCustomProperty(string key);
		

		public ICustomDataProperty GetCustomDataProperty(string customPropertyName, string dataName);

		public ICustomDataProperty<T> GetCustomDataProperty<T>(string customPropertyName, string dataName);

		public dynamic GetCustomDataValue(string customPropertyName, string dataName);
		
		public T GetCustomDataValue<T>(string customPropertyName, string dataName);

		public IUnRegister RegisterOnCustomDataChanged(string customPropertyName, string dataName,
			Action<ICustomDataProperty, object, object> onCustomDataChanged);

		public IUnRegister RegisterOnCustomDataChanged(string customPropertyName, Action<ICustomProperty> onCustomDataChanged);

		public void UnRegisterOnCustomDataChanged(string customPropertyName, string dataName,
			Action<ICustomDataProperty, object, object> onCustomDataChanged);

		public void UnRegisterOnCustomDataChanged(string customPropertyName, Action<ICustomProperty> onCustomDataChanged);

		public bool HasCustomProperty(string propertyName);

	}
}