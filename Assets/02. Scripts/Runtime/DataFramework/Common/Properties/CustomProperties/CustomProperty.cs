using System;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Newtonsoft.Json.Linq;
using Runtime.DataFramework.Description;
using UnityEngine;

namespace Runtime.DataFramework.Properties.CustomProperties{
	
	class CustomPropertyDataUnRegister : IUnRegister
	{
		public ICustomProperty CustomProperty { get; set; }

		public Action<ICustomDataProperty, dynamic, dynamic> OnCustomDataChanged { get; set; }
		
		public string CustomDataName { get; set; }

		public CustomPropertyDataUnRegister(ICustomProperty customProperty, string customDataName, 
			Action<ICustomDataProperty, dynamic, dynamic> onCustomDataChanged)
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

	public interface ICustomProperty : IPropertyDictionary<string, ICustomDataProperty> {
		public string GetCustomPropertyName();
		public string OnGetDescription();

		public ICustomDataProperty GetCustomDataProperty(string customDataName);

		public ICustomDataProperty<T> GetCustomDataProperty<T>(string customDataName);
		
		public IBindableProperty GetCustomDataValue(string customDataName);

		public BindableProperty<T> GetCustomDataValue<T>(string CustomDataName);
		
		public IUnRegister RegisterOnCustomDataChanged(string CustomDataName, Action<ICustomDataProperty, dynamic, dynamic> onCustomDataChanged);

		public IUnRegister RegisterOnCustomDataChanged(Action<ICustomProperty> onCustomDataChanged);
		
		public void UnRegisterOnCustomDataChanged(string CustomDataName, Action<ICustomDataProperty, dynamic, dynamic> onCustomDataChanged);

		public void UnRegisterOnCustomDataChanged(Action<ICustomProperty> onCustomDataChanged);

		PropertyName IPropertyBase.PropertyName => PropertyName.custom_property;

		public Dictionary<string, ICustomDataProperty> OnGetBaseValueFromConfig(dynamic value);
	}
	
	public class CustomProperty : PropertyDictionary<string, ICustomDataProperty>, ICustomProperty {
		
		private Dictionary<ICustomDataProperty, Action<dynamic, dynamic>> onCustomDataChangedInternalCallbacks =
			new Dictionary<ICustomDataProperty, Action<dynamic, dynamic>>();
		
		private Dictionary<string, Action<ICustomDataProperty,dynamic,dynamic>> onCustomDataChangedCallbacks =
			new Dictionary<string, Action<ICustomDataProperty, dynamic, dynamic>>();
		
		private Action<ICustomProperty> onCustomDataChanged;

		[ES3Serializable] protected string propertyName;
		[ES3Serializable] protected IDescriptionGetter<ICustomProperty> descriptionGetter;


		public CustomProperty() : base(){}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="propertyName">The name of the property. This should be the same as the one on the config file</param>
		/// <param name="descriptionGetter">In order to serialize the description, you need to use a descriptionGetter object to do the description stuff</param>
		/// <param name="data">Manually specify which data are included, as well as their dependencies and modifiers</param>
		public CustomProperty(string propertyName, IDescriptionGetter<ICustomProperty> descriptionGetter,
			params ICustomDataProperty[] data) : base() {
			if (data!=null) {
				BaseValue = data.ToDictionary(GetKey);
			}
			else {
				BaseValue = new Dictionary<string, ICustomDataProperty>();
			}
			this.propertyName = propertyName;
			this.descriptionGetter = descriptionGetter;
			
		}
		
		//public abstract ICustomDataProperty[] GetCustomDataProperties();
		
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

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}

		public override string GetKey(ICustomDataProperty value) {
			return value.CustomDataName;
		}
		
		public IBindableProperty GetCustomDataValue(string customDataName) {
			if (!RealValues.Value.ContainsKey(customDataName)) {
				Debug.LogError("CustomDataProperty " + customDataName + " not found in Custom " +  GetCustomPropertyName());
				return null;
			}
			return RealValues[customDataName].GetRealValue();
		}

		public BindableProperty<T> GetCustomDataValue<T>(string CustomDataName) {
			if (!RealValues.Value.ContainsKey(CustomDataName)) {
				Debug.LogError("CustomDataProperty " + CustomDataName + " not found in Custom " +  GetCustomPropertyName());
				return default;
			}

			return RealValues[CustomDataName].GetRealValue() as BindableProperty<T>;
		}

		public string GetCustomPropertyName() {
			return propertyName;
		}

		public string OnGetDescription() {
			return descriptionGetter.GetDescription(this);
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
			Action<ICustomDataProperty, dynamic, dynamic> onCustomDataChanged) {
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

		public void UnRegisterOnCustomDataChanged(string CustomDataName, Action<ICustomDataProperty, dynamic, dynamic> onCustomDataChanged) {
			if (onCustomDataChangedCallbacks.ContainsKey(CustomDataName)) {
				onCustomDataChangedCallbacks[CustomDataName] -= onCustomDataChanged;
			}
		}

		public void UnRegisterOnCustomDataChanged(Action<ICustomProperty> onCustomDataChanged) {
			this.onCustomDataChanged -= onCustomDataChanged;
		}

		public override void Initialize(IPropertyBase[] dependencies, string parentEntityName) {
			base.Initialize(dependencies, parentEntityName);
			RegisterCustomDataValueChangedEvents();
			
		}

		public void OnLoadFromSavedData() {
			base.OnLoadFromSavedData();
			RegisterCustomDataValueChangedEvents();
		}

		private void RegisterCustomDataValueChangedEvents() {
			foreach (ICustomDataProperty CustomData in RealValues.Value.Values) {
				void OnCustomValueChanged(dynamic oldValue, dynamic newValue) => OnCustomDataValueChanged(CustomData, oldValue, newValue);

				CustomData.GetRealValue().RegisterWithInitObject((Action<dynamic, dynamic>) OnCustomValueChanged);
				onCustomDataChangedInternalCallbacks.Add(CustomData, OnCustomValueChanged);
			}
		}

		private void OnCustomDataValueChanged(ICustomDataProperty customData, dynamic oldValue, dynamic newValue) {
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
	/// If you don't want to manually specify custom property data, you can use this class to create a Custom property.
	/// However, this class is data only, which means you can't set any default modifiers for any Custom data (although you can use the builder to override, but the generic type of your modifier MUST BE dynamic in this case!!).
	/// Also, data is only loaded from config file.
	/// Moreover, all Custom data in the config file will be automatically loaded into this class, with type of dynamic.
	/// To customize the description of this Custom, you need to assign a data only Custom description getter.
	/// This is particularly useful if ALL of its data is primitive type, and you don't want to manually specify them.
	/// If any of its data is complex type, the complex data will be a dynamic object (ExpandoObject). So it's better to use CustomProperty instead.
	/// </summary>
	public class AutoConfigCustomProperty : CustomProperty {
		
		public AutoConfigCustomProperty(): base(){}
		
		public AutoConfigCustomProperty(string CustomName, IDescriptionGetter<ICustomProperty> descriptionGetter = null) : base(CustomName, descriptionGetter) {
			
		}

		
		public override Dictionary<string, ICustomDataProperty> OnGetBaseValueFromConfig(dynamic value) {
			BaseValue.Clear();
			IEnumerable<string> keys = (value as JObject)?.Properties().Select(p => p.Name);
			if (keys != null) {
				foreach (string key in keys) {
					if (BaseValue.TryGetValue(key, out ICustomDataProperty val)) {
						val.SetBaseValue(val.OnGetBaseValueFromConfig(value[key]));
					}
					else {
						BaseValue.Add(key, new CustomDataProperty<dynamic>(key));
						ICustomDataProperty bv = this.BaseValue[key];
						bv.SetBaseValue(bv.OnGetBaseValueFromConfig(value[key]));
						requestRegisterProperty?.Invoke(bv.GetType(), bv, GetFullName()+"."+key, true, false);
					}
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