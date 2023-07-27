using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace _02._Scripts.Runtime.Base.Property {
	/// <summary>
	/// Use ListProperty instead if your values are not properties. If your values are properties, use this class
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class PropertyList<T> : Property<List<T>>, IListProperty<T>, IHaveSubProperties  where T: IPropertyBase {
		/// <summary>
		/// Use RealValues instead to invoke events
		/// </summary>
		public override BindableProperty<List<T>> RealValue => RealValues;

		[field: ES3Serializable]
		public BindableList<T> RealValues { get; private set; } = new BindableList<T>();

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
		
		public void AddToRealValue(T property) {
			//parentEntity.RegisterTempProperty(property, fullName + "[" + RealValues.Count() + "]");
			requestRegisterProperty?.Invoke(typeof(T), property, fullName + "[" + RealValues.Count() + "]", true, true);
			
			RealValues.AddAndInvoke(property);
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
		

		public override PropertyNameInfo[] GetDependentProperties() {
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
			
			base.OnRecycled();
		}
	}
}