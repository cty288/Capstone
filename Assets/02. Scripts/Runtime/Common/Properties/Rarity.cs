using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Property;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IRarityProperty : IProperty<int>, ILoadFromConfigProperty {
	
	}
	
	public class Rarity : IndependentLoadFromConfigProperty<int>, IRarityProperty {

		public override int OnSetBaseValueFromConfig(dynamic value) {
			return value;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.rarity;
		}
	}
}