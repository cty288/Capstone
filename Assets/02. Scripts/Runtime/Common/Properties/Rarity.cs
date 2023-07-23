using System.Collections.Generic;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IRarityProperty : IProperty<int>{}
	
	public class Rarity : IndependentProperty<int>, IRarityProperty {
		public override int OnSetBaseValueFromConfig(dynamic value) {
			return value;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.rarity;
		}
	}
}