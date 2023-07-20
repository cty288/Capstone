using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Base.Property;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.Properties {
	public enum TasteType {
		Type1,
		Type2,
		Type3
	}
	
	public interface ITasteProperty : IListProperty<TasteType> {}
	public class Taste : IndependentListProperty<TasteType>, ITasteProperty {
		public override List<TasteType> OnSetBaseValueFromConfig(dynamic value) {
			return value.ToObject<List<TasteType>>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.taste;
		}
		
		public Taste(): base(){}
		
		public Taste(params TasteType[] baseValues) : base() {
			BaseValue = baseValues.ToList();
		}
	}
}