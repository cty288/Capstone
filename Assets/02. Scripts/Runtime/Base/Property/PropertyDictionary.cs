using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace _02._Scripts.Runtime.Base.Property {
	public interface IDictionaryProperty<TKey,T> : IProperty<Dictionary<TKey, T>> {
		BindableDictionary<TKey, T> RealValues { get; }

		public TKey GetKey(T value);

		public void AddToRealValue(T property, IEntity parentEntity);
		
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
		
		
		public override void Initialize(IPropertyBase[] dependencies, string parentEntityName) {
			if (BaseValue != null) {
				foreach (var property in BaseValue) {
					property.Value.Initialize(dependencies, parentEntityName);
				}

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

		public override PropertyName[] GetDependentProperties() {
			List<PropertyName> dependentProperties = new List<PropertyName>();
			if (BaseValue != null) {
				foreach (var property in BaseValue) {
					dependentProperties.AddRange(property.Value.GetDependentProperties());
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
}