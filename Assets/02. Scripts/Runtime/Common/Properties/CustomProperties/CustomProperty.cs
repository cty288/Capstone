using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Base.Property;
using _02._Scripts.Runtime.Common.Properties.SkillsBase;
using MikroFramework.Event;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.Properties{
	
	class CustomPropertyDataUnRegister : IUnRegister
	{
		public ICustomProperty CustomProperty { get; set; }

		public Action<ICustomDataProperty, object, object> OnCustomDataChanged { get; set; }
		
		public string CustomDataName { get; set; }

		public CustomPropertyDataUnRegister(ICustomProperty customProperty, string customDataName, 
			Action<ICustomDataProperty, object, object> onCustomDataChanged)
		{
			this.CustomProperty = customProperty;
			this.OnCustomDataChanged = onCustomDataChanged;
			this.CustomDataName = customDataName;
		}

		public void UnRegister() {
			CustomProperty.UnRegisterOnCustomDataChanged(CustomDataName, OnCustomDataChanged);
			CustomProperty = null;
		}
	}
	
	class CustomPropertyUnRegister : IUnRegister
	{
		public ICustomProperty CustomProperty { get; set; }

		public Action<ICustomProperty> OnCustomDataChanged { get; set; }
		

		public CustomPropertyUnRegister(ICustomProperty customProperty, Action<ICustomProperty> onCustomDataChanged) {
			this.CustomProperty = customProperty;
			this.OnCustomDataChanged = onCustomDataChanged;
		}

		public void UnRegister() {
			CustomProperty.UnRegisterOnCustomDataChanged(OnCustomDataChanged);
			CustomProperty = null;
		}
	}

	public interface ICustomProperty : IDictionaryProperty<string, ICustomDataProperty> {
		public string GetCustomPropertyName();
		public string OnGetDescription();

		public ICustomDataProperty GetCustomDataProperty(string customDataName);

		public ICustomDataProperty<T> GetCustomDataProperty<T>(string customDataName);
		
		public dynamic GetCustomDataValue(string customDataName);

		public T GetCustomDataValue<T>(string CustomDataName);
		
		public IUnRegister RegisterOnCustomDataChanged(string CustomDataName, Action<ICustomDataProperty, object, object> onCustomDataChanged);

		public IUnRegister RegisterOnCustomDataChanged(Action<ICustomProperty> onCustomDataChanged);
		
		public void UnRegisterOnCustomDataChanged(string CustomDataName, Action<ICustomDataProperty, object, object> onCustomDataChanged);

		public void UnRegisterOnCustomDataChanged(Action<ICustomProperty> onCustomDataChanged);

		PropertyName IPropertyBase.PropertyName => PropertyName.custom_property;

		public Dictionary<string, ICustomDataProperty> OnGetBaseValueFromConfig(dynamic value);
	}
	
	public abstract class AbstractCustomProperty : PropertyDictionary<string, ICustomDataProperty>, ICustomProperty {
		
		private Dictionary<ICustomDataProperty, Action<object, object>> onCustomDataChangedInternalCallbacks =
			new Dictionary<ICustomDataProperty, Action<object, object>>();
		
		private Dictionary<string, Action<ICustomDataProperty,object,object>> onCustomDataChangedCallbacks =
			new Dictionary<string, Action<ICustomDataProperty, object, object>>();
		
		private Action<ICustomProperty> onCustomDataChanged;

		public abstract string GetCustomPropertyName();
		public abstract string OnGetDescription();



		public AbstractCustomProperty() : base() {
			
			ICustomDataProperty[] CustomDataProperties = GetCustomDataProperties();
			if (CustomDataProperties!=null) {
				BaseValue = GetCustomDataProperties().ToDictionary(GetKey);
			}
			else {
				BaseValue = new Dictionary<string, ICustomDataProperty>();
			}
			
		}
		
		public abstract ICustomDataProperty[] GetCustomDataProperties();
		
		public virtual Dictionary<string, ICustomDataProperty> OnGetBaseValueFromConfig(dynamic value) {
			IEnumerable<string> keys = (value as JObject)?.Properties().Select(p => p.Name);
			if (keys != null)
				foreach (string key in keys) {
					if (BaseValue.TryGetValue(key, out ICustomDataProperty val)) {
						val.SetBaseValue(val.OnGetBaseValueFromConfig(value[key]));
					}
					else {
						Debug.LogError("CustomDataProperty " + key + " not found in Custom " + GetCustomPropertyName());
					}
				}

			return BaseValue;
		}

		protected override IPropertyDependencyModifier<Dictionary<string, ICustomDataProperty>> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.custom_property;
		}

		public override string GetKey(ICustomDataProperty value) {
			return value.CustomDataName;
		}
		
		public dynamic GetCustomDataValue(string customDataName) {
			if (!RealValues.Value.ContainsKey(customDataName)) {
				Debug.LogError("CustomDataProperty " + customDataName + " not found in Custom " +  GetCustomPropertyName());
				return null;
			}
			return RealValues[customDataName].GetRealValue().Value;
		}

		public T GetCustomDataValue<T>(string CustomDataName) {
			if (!RealValues.Value.ContainsKey(CustomDataName)) {
				Debug.LogError("CustomDataProperty " + CustomDataName + " not found in Custom " +  GetCustomPropertyName());
				return default;
			}

			return (T) RealValues[CustomDataName].GetRealValue().Value;
		}
		
		public ICustomDataProperty GetCustomDataProperty(string customDataName) {
			if (!RealValues.Value.ContainsKey(customDataName)) {
				Debug.LogError("CustomDataProperty " + customDataName + " not found in Custom " +  GetCustomPropertyName());
				return null;
			}
			return RealValues[customDataName];
		}

		public ICustomDataProperty<T> GetCustomDataProperty<T>(string customDataName) {
			if (!RealValues.Value.ContainsKey(customDataName)) {
				Debug.LogError("CustomDataProperty " + customDataName + " not found in Custom " +  GetCustomPropertyName());
				return null;
			}
			return (ICustomDataProperty<T>) RealValues[customDataName];
		}

		public IUnRegister RegisterOnCustomDataChanged(string CustomDataName, 
			Action<ICustomDataProperty, object, object> onCustomDataChanged) {
			if (onCustomDataChangedCallbacks.ContainsKey(CustomDataName)) {
				onCustomDataChangedCallbacks[CustomDataName] += onCustomDataChanged;
			}
			else {
				onCustomDataChangedCallbacks.Add(CustomDataName, onCustomDataChanged);
			}
			return new CustomPropertyDataUnRegister(this, CustomDataName, onCustomDataChanged);
		}

		public IUnRegister RegisterOnCustomDataChanged(Action<ICustomProperty> onCustomDataChanged) {
			this.onCustomDataChanged += onCustomDataChanged;
			return new CustomPropertyUnRegister(this, this.onCustomDataChanged);
		}

		public void UnRegisterOnCustomDataChanged(string CustomDataName, Action<ICustomDataProperty, object, object> onCustomDataChanged) {
			if (onCustomDataChangedCallbacks.ContainsKey(CustomDataName)) {
				onCustomDataChangedCallbacks[CustomDataName] -= onCustomDataChanged;
			}
		}

		public void UnRegisterOnCustomDataChanged(Action<ICustomProperty> onCustomDataChanged) {
			this.onCustomDataChanged -= onCustomDataChanged;
		}

		public override void Initialize(IPropertyBase[] dependencies, string parentEntityName) {
			base.Initialize(dependencies, parentEntityName);
			foreach (ICustomDataProperty CustomData in RealValues.Value.Values) {
				Action<object, object> onCustomValueChanged = (oldValue, newValue) =>
					OnCustomDataValueChanged(CustomData, oldValue, newValue);

				CustomData.GetRealValue().RegisterWithInitObject(onCustomValueChanged);
				onCustomDataChangedInternalCallbacks.Add(CustomData, onCustomValueChanged);
			}
		}

		private void OnCustomDataValueChanged(ICustomDataProperty customData, object oldValue, object newValue) {
			if (onCustomDataChangedCallbacks.TryGetValue(customData.CustomDataName, out var callback)) {
				callback.Invoke(customData, oldValue, newValue);
			}
		}

		public override void OnRecycled() {
			base.OnRecycled();
			foreach (ICustomDataProperty CustomProperty in onCustomDataChangedInternalCallbacks.Keys) {
				CustomProperty.GetRealValue().UnRegisterOnObjectValueChanged(onCustomDataChangedInternalCallbacks[CustomProperty]);
			}
			onCustomDataChangedInternalCallbacks.Clear();
			onCustomDataChangedCallbacks.Clear();
		}
	}


	/// <summary>
	/// If you don't want to create a new class for each Custom Property, you can use this class to create a Custom property.
	/// However, this class is data only, which means you can't set any modifiers for any Custom data. Also, data is only loaded from config file.
	/// Moreover, all Custom data in the config file will be automatically loaded into this class, with type of dynamic.
	/// To customize the description of this Custom, you need to assign a data only Custom description getter
	/// </summary>
	public class DataOnlyCustomProperty : AbstractCustomProperty {
		[ES3Serializable] 
		protected string customPropertyName;
		
		[field: ES3Serializable]
		protected IDataOnlyDescriptionGetter DescriptionGetter { get; }
		
		public DataOnlyCustomProperty(): base(){}
		
		public DataOnlyCustomProperty(string CustomName, IDataOnlyDescriptionGetter descriptionGetter = null) : base() {
			this.customPropertyName = CustomName;
			DescriptionGetter = descriptionGetter;
		}

		public override string GetCustomPropertyName() {
			return customPropertyName;
		}

		public override string OnGetDescription() {
			if (DescriptionGetter != null) {
				return DescriptionGetter.GetDescription(this);
			}

			return "";
		}

		public override ICustomDataProperty[] GetCustomDataProperties() {
			return null;
		}

		public override Dictionary<string, ICustomDataProperty> OnGetBaseValueFromConfig(dynamic value) {
			BaseValue.Clear();
			IEnumerable<string> keys = (value as JObject)?.Properties().Select(p => p.Name);
			if (keys != null)
				foreach (string key in keys) {
					if (BaseValue.TryGetValue(key, out ICustomDataProperty val)) {
						val.SetBaseValue(val.OnGetBaseValueFromConfig(value[key]));
					}
					else {
						BaseValue.Add(key, new CustomDataProperty<dynamic>(key));
						ICustomDataProperty bv = this.BaseValue[key];
						bv.SetBaseValue(bv.OnGetBaseValueFromConfig(value[key]));
					}
				}

			return BaseValue;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			BaseValue.Clear();
		}
	}



}