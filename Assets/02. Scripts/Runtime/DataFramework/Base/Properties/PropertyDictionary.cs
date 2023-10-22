using System;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.Utilities;

namespace Runtime.DataFramework.Properties {
	public interface IPropertyDictionary<TKey,T> : IDictionaryProperty<TKey, T>, IProperty<Dictionary<TKey, T>>, IHaveSubProperties {

		public TKey GetKey(T value);

		public void AddToRealValue(T property);
		
		public void RemoveFromRealValue(TKey key);
	}
	public abstract class PropertyDictionary<TKey,T> : Property<Dictionary<TKey, T>>, IPropertyDictionary<TKey,T> where T: IPropertyBase {

		[ES3Serializable]
		private Dictionary<T, SerializedChildPropertyInfo> _childProperties =
			new Dictionary<T, SerializedChildPropertyInfo>();

		[field: ES3NonSerializable]
		public override Dictionary<TKey, T> BaseValue { get; set; } = new Dictionary<TKey, T>();

		/// <summary>
		/// Use RealValues instead to invoke events
		/// </summary>
		public override BindableProperty<Dictionary<TKey, T>> RealValue => RealValues;

		[field: ES3NonSerializable]
		public BindableDictionary<TKey, T> RealValues { get; private set; } =
			new BindableDictionary<TKey, T>(new Dictionary<TKey, T>());

		[field: ES3NonSerializable]
		public override Dictionary<TKey, T> InitialValue { get; set; } = new Dictionary<TKey, T>();

		public abstract TKey GetKey(T value);


		public PropertyDictionary() : base() {
			BaseValue = new Dictionary<TKey, T>();
		}
		
		public PropertyDictionary(params T[] baseValues) : this() {
			if (baseValues != null) {
				BaseValue = baseValues.ToDictionary(GetKey);
			}
		
		}
		public void OnLoadFromSavedData() {
			BaseValue.Clear();
			InitialValue.Clear();
			RealValues.Value.Clear();
			
			foreach (KeyValuePair<T,SerializedChildPropertyInfo> childProperty in _childProperties) {
				SerializedChildPropertyInfo info = childProperty.Value;
				TKey key = GetKey(childProperty.Key);
				
				if (info.IsInBase) {
					BaseValue.Add(key, childProperty.Key);
					InitialValue.Add(key, childProperty.Key);
				}
				
				if(info.IsInReal) {
					RealValues.Value.Add(key, childProperty.Key);
				}
				
				if(childProperty.Key is IHaveSubProperties subProperty) {
					subProperty.OnLoadFromSavedData();
				}
			}
		}
		
		public override void SetBaseValue(Dictionary<TKey, T> value) {
			BaseValue = value;
		}

		public void OnSetChildFullName() {
			if (BaseValue == null) {
				return;
			}
			foreach (KeyValuePair<TKey,T> keyValuePair in BaseValue) {
				keyValuePair.Value.OnSetFullName(fullName + "." + keyValuePair.Key);
			}
		}


		public void AddToRealValue(T property) {
			//parentEntity.RegisterTempProperty(property, fullName + "." + GetKey(property));
			requestRegisterProperty?.Invoke(typeof(T), property, fullName + "." + GetKey(property), true, true);
			RealValues.AddAndInvoke(GetKey(property), property);
			_childProperties.Add(property, new SerializedChildPropertyInfo(false, true));
		}

		public void RemoveFromRealValue(TKey key) {
			if(RealValues.Value.ContainsKey(key)) {
				T property = RealValues.Value[key];
				RealValues.RemoveAndInvoke(key);
				SerializedChildPropertyInfo childPropertyInfo = _childProperties[property];
				if (childPropertyInfo != null) {
					childPropertyInfo.IsInReal = false;
				}
			}
			
		}

		/// <summary>
		/// Shallow clone of the list
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected override Dictionary<TKey, T> OnClone(Dictionary<TKey, T> value) {
			Dictionary<TKey, T> clone = new Dictionary<TKey, T>();
			if (value != null) {
				foreach (var property in value) {
					clone.Add(property.Key, property.Value);
				}
			}
			
			return clone;
		}

		public override void Initialize(IPropertyBase[] dependencies, string parentEntityName) {
			base.Initialize(dependencies, parentEntityName);
			foreach (KeyValuePair<TKey,T> keyValuePair in BaseValue) {
				_childProperties.Add(keyValuePair.Value, new SerializedChildPropertyInfo(true, true));
			}
		}

		public IPropertyBase[] GetChildProperties() {
			if (BaseValue != null) {
				List<IPropertyBase> childProperties = new List<IPropertyBase>();
				foreach (T baseVal in BaseValue.Values) {
					childProperties.Add(baseVal);
					if(baseVal is IHaveSubProperties haveSubProperties) {
						IPropertyBase[] subProperties = haveSubProperties.GetChildProperties();
						if (subProperties != null) {
							childProperties.AddRange(subProperties);
						}
					}
				}
				return childProperties.ToArray();
			}
			return null;
		}
		
		protected Action<Type, IPropertyBase, string, bool, bool> requestRegisterProperty;
		public void RegisterRequestRegisterProperty(Action<Type, IPropertyBase, string, bool, bool> requestRegisterProperty) {
			this.requestRegisterProperty += requestRegisterProperty;
		}

		public void UnregisterRequestRegisterProperty(Action<Type, IPropertyBase, string, bool, bool> requestRegisterProperty) {
			this.requestRegisterProperty -= requestRegisterProperty;
		}



		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			List<PropertyNameInfo> dependentProperties = new List<PropertyNameInfo>();
			if (BaseValue != null) {
				foreach (var property in BaseValue) {
					PropertyNameInfo[] dependentPropertyNames = property.Value.GetDependentProperties();
					if (dependentPropertyNames != null) {
						dependentProperties.AddRange(dependentPropertyNames);
					}
					
				}
			}
			

			return dependentProperties.ToArray();
		}

		public override void OnRecycled() {
			if (BaseValue != null) {
				foreach (var property in BaseValue) {
					property.Value.OnRecycled();
				}
			}
			_childProperties.Clear();
			base.OnRecycled();
		}
	}
	
	
	public abstract class PropertyDictionaryLoadFromConfig<TKey,T> : PropertyDictionary<TKey,T>, ILoadFromConfigProperty where T: IPropertyBase {
		
		public void LoadFromConfig(dynamic value, IEntity parentEntity){
			if (value != null) {
				SetBaseValue(OnSetBaseValueFromConfig(value, parentEntity));
			}
		}
	
		public abstract Dictionary<TKey, T> OnSetBaseValueFromConfig(dynamic value, IEntity parentEntity);
	
		public PropertyDictionaryLoadFromConfig() : base() {
		
		}
		
	}
}