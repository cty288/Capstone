using System;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.BindableProperty;
using Runtime.Utilities;

namespace Runtime.DataFramework.Properties {
	/// <summary>
	/// Use ListProperty instead if your values are not properties. If your values are properties, use this class
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class PropertyList<T> : Property<List<T>>, IListProperty<T>, IHaveSubProperties  where T: IPropertyBase {
		
		[ES3Serializable]
		private Dictionary<T, SerializedChildPropertyInfo> _childProperties =
			new Dictionary<T, SerializedChildPropertyInfo>();

		[field: ES3NonSerializable]
		public override List<T> BaseValue { get; set; } = new List<T>();

		/// <summary>
		/// Use RealValues instead to invoke events
		/// </summary>
		public override BindableProperty<List<T>> RealValue => RealValues;

		[field: ES3NonSerializable]
		public BindableList<T> RealValues { get; private set; } = new BindableList<T>();
		
		
		[field: ES3NonSerializable]
		public override List<T> InitialValue { get; set; } = new List<T>();

		public PropertyList() : base() {
			
		}
		
		public PropertyList(params T[] baseValues) : this() {
			BaseValue = baseValues.ToList();
		}

		public override void SetBaseValue(List<T> value) {
			BaseValue = value;
		}
		
		protected Action<Type, IPropertyBase, string, bool, bool> requestRegisterProperty;
		public void RegisterRequestRegisterProperty(Action<Type, IPropertyBase, string, bool, bool> requestRegisterProperty) {
			this.requestRegisterProperty += requestRegisterProperty;
		}

		public void UnregisterRequestRegisterProperty(Action<Type, IPropertyBase, string, bool, bool> requestRegisterProperty) {
			this.requestRegisterProperty -= requestRegisterProperty;
		}

		public void OnLoadFromSavedData() {
			BaseValue.Clear();
			InitialValue.Clear();
			RealValues.Value.Clear();
			
			foreach (KeyValuePair<T,SerializedChildPropertyInfo> childProperty in _childProperties) {
				SerializedChildPropertyInfo info = childProperty.Value;
				
				if (info.IsInBase) {
					BaseValue.Add(childProperty.Key);
					InitialValue.Add(childProperty.Key);
				}
				
				if(info.IsInReal) {
					RealValues.Value.Add(childProperty.Key);
				}
				
				if(childProperty.Key is IHaveSubProperties subProperty) {
					subProperty.OnLoadFromSavedData();
				}
			}
		}

		public void AddToRealValue(T property) {
			//parentEntity.RegisterTempProperty(property, fullName + "[" + RealValues.Count() + "]");
			requestRegisterProperty?.Invoke(typeof(T), property, fullName + "[" + RealValues.Count() + "]", true, true);
			
			RealValues.AddAndInvoke(property);
			
			_childProperties.Add(property, new SerializedChildPropertyInfo(false, true));
		}



		public void OnSetChildFullName() {
			if (BaseValue != null) {
				for (int i = 0; i < BaseValue.Count; i++) {
					BaseValue[i].OnSetFullName(fullName + "[" + i + "]");
				}
			}
		}

		public IPropertyBase[] GetChildProperties() {
			if (BaseValue is {Count: > 0}) {
				List<IPropertyBase> childProperties = new List<IPropertyBase>();
				foreach (T baseVal in BaseValue) {
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

		/// <summary>
		/// Shallow clone of the list
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected override List<T> OnClone(List<T> value) { 
			List<T> clone = new List<T>();
			foreach (var property in value) {
				clone.Add(property);
			}
			return clone;
		}

		public override void Initialize(IPropertyBase[] dependencies, string parentEntityName) {
			base.Initialize(dependencies, parentEntityName);
			foreach (T property in BaseValue) {
				_childProperties.Add(property, new SerializedChildPropertyInfo(true, true));
			}
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			List<PropertyNameInfo> dependentProperties = new List<PropertyNameInfo>();
			if (BaseValue != null) {
				foreach (var property in BaseValue) {
					PropertyNameInfo[] dependentPropertyNames = property.GetDependentProperties();
					if (dependentPropertyNames != null) {
						dependentProperties.AddRange(dependentPropertyNames);
					}
					
				}
			}
			

			return dependentProperties.ToArray();
			
		}

		public override void OnRecycled() {
			if (BaseValue!=null) {
				foreach (var property in BaseValue) {
					property.OnRecycled();
				}
			}
			_childProperties.Clear();
			base.OnRecycled();
		}
	}
}