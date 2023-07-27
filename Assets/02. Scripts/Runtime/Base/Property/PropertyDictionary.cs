using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace _02._Scripts.Runtime.Base.Property {
	public interface IDictionaryProperty<TKey,T> : IProperty<Dictionary<TKey, T>>, IHaveSubProperties {
		BindableDictionary<TKey, T> RealValues { get; }

		public TKey GetKey(T value);

		public void AddToRealValue(T property);
		
		public void RemoveFromRealValue(TKey key);
	}
	public abstract class PropertyDictionary<TKey,T> : Property<Dictionary<TKey, T>>, IDictionaryProperty<TKey,T> where T: IPropertyBase {
		/// <summary>
		/// Use RealValues instead to invoke events
		/// </summary>
		public override BindableProperty<Dictionary<TKey, T>> RealValue => RealValues;

		[field: ES3Serializable]
		public BindableDictionary<TKey, T> RealValues { get; private set; } =
			new BindableDictionary<TKey, T>();

		public abstract TKey GetKey(T value);


		public PropertyDictionary() : base() {
			
		}
		
		public PropertyDictionary(params T[] baseValues) : this() {
			if (baseValues != null) {
				BaseValue = baseValues.ToDictionary(GetKey);
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
		}

		public void RemoveFromRealValue(TKey key) {
			RealValues.RemoveAndInvoke(key);
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

		public override PropertyNameInfo[] GetDependentProperties() {
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
			
			base.OnRecycled();
		}
	}
	
	
	public abstract class PropertyDictionaryLoadFromConfig<TKey,T> : PropertyDictionary<TKey,T>, ILoadFromConfigProperty where T: IPropertyBase {
		
		public void LoadFromConfig(dynamic value) {
			if (value != null) {
				SetBaseValue(OnSetBaseValueFromConfig(value));
			}
		}
	
		public abstract Dictionary<TKey, T> OnSetBaseValueFromConfig(dynamic value);
	
		public PropertyDictionaryLoadFromConfig() : base() {
		
		}
		
	}
}