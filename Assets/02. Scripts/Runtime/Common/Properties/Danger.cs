using System.Collections.Generic;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IDangerProperty : IProperty<int>{}
	public class Danger: Property<int>, IDangerProperty {
		
		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return new DangerDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.danger;
		}

		public override PropertyName[] GetDependentProperties() {
			return new[] {PropertyName.rarity};
		}
		
		
	}
	
	public class DangerDefaultModifier : PropertyDependencyModifier<int> {
		public override int OnModify(int propertyValue) {
			return GetDependency<Rarity>().InitialValue * 5;
		}
	}
}