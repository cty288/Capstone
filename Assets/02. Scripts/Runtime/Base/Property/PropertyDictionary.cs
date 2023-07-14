using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace _02._Scripts.Runtime.Base.Property {
	public interface IDictionaryProperty<T> : IProperty<Dictionary<PropertyName, T>> {
		BindableDictionary<PropertyName, T> RealValues { get; }
	}
	public abstract class PropertyDictionary<T> : Property<Dictionary<PropertyName, T>>, IDictionaryProperty<T> where T: IPropertyBase {
		/// <summary>
		/// Use RealValues instead to invoke events
		/// </summary>
		public override BindableProperty<Dictionary<PropertyName, T>> RealValue => RealValues;

		[field: ES3Serializable]
		public BindableDictionary<PropertyName, T> RealValues { get; private set; } =
			new BindableDictionary<PropertyName, T>();

		public PropertyDictionary() : base() {
			
		}
		
		public PropertyDictionary(params T[] baseValues) : this() {
			BaseValue = baseValues.ToDictionary(p => p.PropertyName);
		}

		
		public override void SetBaseValue(Dictionary<PropertyName, T> value) {
			BaseValue = value;
		}
		
		
		public override void Initialize(IPropertyBase[] dependencies, string parentEntityName) {
			foreach (var property in BaseValue) {
				property.Value.Initialize(dependencies, parentEntityName);
			}
			
			base.Initialize(dependencies, parentEntityName);
		}
		
		public void AddToRealValue(T property, IEntity parentEntity) {
			PropertyName[] dependencies = property.GetDependentProperties();
			IPropertyBase[] dependencyProperties = new IPropertyBase[dependencies.Length];
			for (int i = 0; i < dependencies.Length; i++) {
				var p = parentEntity.GetProperty(dependencies[i]);
				if (p == null) {
					Debug.LogError("Property " + dependencies[i] + " not found in entity " + parentEntity.EntityName);
					return;
				}
				dependencyProperties[i] = p;
			}

			property.Initialize(dependencyProperties, parentEntity.EntityName);
			RealValues.AddAndInvoke(property.PropertyName, property);
		}

		/// <summary>
		/// Shallow clone of the list
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected override Dictionary<PropertyName, T> OnClone(Dictionary<PropertyName, T> value) {
			Dictionary<PropertyName, T> clone = new Dictionary<PropertyName, T>();
			if (value != null) {
				foreach (var property in value) {
					clone.Add(property.Key, property.Value);
				}
			}
			
			return clone;
		}

		public override PropertyName[] GetDependentProperties() {
			List<PropertyName> dependentProperties = new List<PropertyName>();
			foreach (var property in BaseValue) {
				dependentProperties.AddRange(property.Value.GetDependentProperties());
			}

			return dependentProperties.ToArray();
		}

		public override void OnRecycled() {
			foreach (var property in BaseValue) {
				property.Value.OnRecycled();
			}
			base.OnRecycled();
		}
	}
}