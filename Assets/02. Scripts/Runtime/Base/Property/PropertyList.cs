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
	public abstract class PropertyList<T> : Property<List<T>>, IListProperty<T> where T: IPropertyBase {
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
		
		
		public override void Initialize(IPropertyBase[] dependencies, string parentEntityName) {
			foreach (var property in BaseValue) {
				property.Initialize(dependencies, parentEntityName);
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
			RealValues.AddAndInvoke(property);
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

		public override PropertyName[] GetDependentProperties() {
			return BaseValue.SelectMany(p => p.GetDependentProperties()).Distinct().ToArray();
		}

		public override void OnRecycled() {
			foreach (var property in BaseValue) {
				property.OnRecycled();
			}
			base.OnRecycled();
		}
	}
}