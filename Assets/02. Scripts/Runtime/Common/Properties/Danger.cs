using System.Collections.Generic;

namespace _02._Scripts.Runtime.Common.Properties {
	public class Danger: Property<int> {
		
		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return new DangerModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.danger;
		}

		public override PropertyName[] GetDependentProperties() {
			return new[] {PropertyName.rarity};
		}
		
		
	}
	
	public class DangerModifier : PropertyDependencyModifier<int> {
		public override int OnModify(int propertyValue) {
			return GetDependency<Rarity>().InitialValue * 5;
		}
	}
}