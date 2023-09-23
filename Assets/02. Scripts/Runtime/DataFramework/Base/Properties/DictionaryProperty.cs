using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.Utilities;

namespace Runtime.DataFramework.Properties {
	public interface IDictionaryProperty<TKey,T> : IProperty<Dictionary<TKey, T>> {
		BindableDictionary<TKey, T> RealValues { get; }
	}
	
	
	public abstract class DictionaryProperty<TKey,T> : Property<Dictionary<TKey, T>>, IDictionaryProperty<TKey, T> {
		
		/// <summary>
		/// Use RealValues instead to invoke events
		/// </summary>
		public override BindableProperty<Dictionary<TKey,T>> RealValue => RealValues;

		

		public BindableDictionary<TKey,T> RealValues { get; private set; } =
			new BindableDictionary<TKey,T>(new Dictionary<TKey,T>());


		public DictionaryProperty() : base() {
			BaseValue = new Dictionary<TKey, T>();
		}
		
		/// <summary>
		/// Deep clone of the list
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected override Dictionary<TKey, T> OnClone(Dictionary<TKey, T> value) {
			Dictionary<TKey, T> clone = new Dictionary<TKey, T>();

			if (value != null) {
				foreach (var item in value) {
					if (item.Value is ICloneable cloneable) {
						clone.Add(item.Key, (T) cloneable.Clone());
					}
					else {
						clone.Add(item.Key, item.Value);
					}
				}
			}
			return clone;
		}
		
	}
	
	public abstract class LoadFromConfigDictProperty<TKey, T> : DictionaryProperty<TKey, T>, ILoadFromConfigProperty {
	
		public void LoadFromConfig(dynamic value) {
			if (value is not  null) {
				SetBaseValue(OnClone(value));
			}
		}
	
		//public abstract Dictionary<TKey, T>  OnSetBaseValueFromConfig(dynamic value);
		

	}
}