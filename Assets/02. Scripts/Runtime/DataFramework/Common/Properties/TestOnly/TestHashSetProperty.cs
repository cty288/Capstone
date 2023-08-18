using System.Collections.Generic;

namespace Runtime.DataFramework.Properties.TestOnly {
	public class TestHashSetProperty : LoadFromConfigHashsetProperty<string> {
		protected override IPropertyDependencyModifier<HashSet<string>> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.test_hashset;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}