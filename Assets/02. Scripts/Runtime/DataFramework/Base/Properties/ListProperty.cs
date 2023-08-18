using System;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.BindableProperty;
using Runtime.Utilities;

namespace Runtime.DataFramework.Properties {
	public interface IListProperty<T> : IProperty<List<T>> {
		BindableList<T> RealValues { get; }
		
	}
	/// <summary>
	/// Use PropertyList instead if you want the values to be property as well. If your values are not property, use this class
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ListProperty<T> : Property<List<T>>, IListProperty<T>{
		/// <summary>
		/// Use RealValues instead to invoke events
		/// </summary>
		public override BindableProperty<List<T>> RealValue => RealValues;

		[field: ES3Serializable]
		public BindableList<T> RealValues { get; private set; } = new BindableList<T>();

		public ListProperty() : base() {
			
		}
		
		public ListProperty(params T[] baseValues) : this() {
			BaseValue = baseValues.ToList();
		}

		public override void SetBaseValue(List<T> value) {
			BaseValue = value;
		}
		
		

		/// <summary>
		/// Deep clone of the list
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected override List<T> OnClone(List<T> value) {
			List<T> clone = new List<T>();

			if (value != null) {
				foreach (var item in value) {
					if (item is ICloneable cloneable) {
						clone.Add((T)cloneable.Clone());
					}
					else {
						clone.Add(item);
					}
				}
			}
			return clone;
		}
	}


	/// <summary>
	/// Independent Properties are properties without any dependencies
	/// </summary>
	/// <typeparam name="T"></typeparam>

	public abstract class IndependentListProperty<T> : ListProperty<T> {
	
		protected override IPropertyDependencyModifier<List<T>> GetDefautModifier() {
			return null;
		}
	

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}

		protected IndependentListProperty():base(){}
	}

	public abstract class IndependentLoadFromConfigListProperty<T> : IndependentListProperty<T>, ILoadFromConfigProperty {
	
		public void LoadFromConfig(dynamic value) {
			if (value is not null) {
				SetBaseValue(OnClone(value));
			}
		}
	
		//public abstract List<T> OnSetBaseValueFromConfig(dynamic value);
		protected IndependentLoadFromConfigListProperty():base(){}

	}
}