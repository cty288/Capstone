using System.Collections.Generic;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.Properties {
	public class Rarity : IndependentProperty<int> {
		protected override PropertyName GetPropertyName() {
			return PropertyName.rarity;
		}
	}
}