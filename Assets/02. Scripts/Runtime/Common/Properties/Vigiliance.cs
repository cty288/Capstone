using System.Collections.Generic;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IVigilianceProperty : IProperty<float>{}
	public class Vigiliance: Property<float>, IVigilianceProperty {
		public override float OnSetBaseValueFromConfig(dynamic value) {
			return value;
		}

		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new VigilianceDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.vigiliance;
		}

		public override PropertyName[] GetDependentProperties() {
			return new[] {PropertyName.rarity};
		}
		
		
	}
	
	public class VigilianceDefaultModifier : PropertyDependencyModifier<float> {
		public override float OnModify(float propertyValue) {
			return propertyValue * GetDependency<Rarity>().InitialValue * 5;
		}
	}
}