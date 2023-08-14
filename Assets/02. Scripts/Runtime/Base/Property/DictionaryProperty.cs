using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.BindableProperty;

namespace _02._Scripts.Runtime.Base.Property {
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
		
		
		public DictionaryProperty(): base(){}
		
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
			if (value != null) {
				SetBaseValue(OnSetBaseValueFromConfig(value));
			}
		}
	
		public abstract Dictionary<TKey, T>  OnSetBaseValueFromConfig(dynamic value);
		

	}
}