using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.Utilities;

namespace Runtime.DataFramework.Properties {
	
	public interface IHashSetProperty<T> : IProperty<HashSet<T>> {
		BindableHashset<T> RealValues { get; }
		
	}
	public abstract class HashSetProperty<T> : Property<HashSet<T>>, IHashSetProperty<T> {
		public override BindableProperty<HashSet<T>> RealValue => RealValues;
		
		public HashSetProperty() : base() {
			BaseValue = new HashSet<T>();
		}
		
	

		[field: ES3Serializable]
		public BindableHashset<T> RealValues { get; private set; } = new BindableHashset<T>();
		
		public override void SetBaseValue(HashSet<T> value) {
			BaseValue = value;
		}

		protected override HashSet<T> OnClone(HashSet<T> value) {
			HashSet<T> clone = new HashSet<T>();
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
	
	public abstract class LoadFromConfigHashsetProperty<T> : HashSetProperty<T>, ILoadFromConfigProperty {
	
		public void LoadFromConfig(dynamic value, IEntity parentEntity) {
			if (value is not null) {
				SetBaseValue(OnClone(value));
			}
		}
	

		
		public LoadFromConfigHashsetProperty() : base() {
			
		}
		

	}
}