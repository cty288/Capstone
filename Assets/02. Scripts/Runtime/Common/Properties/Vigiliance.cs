using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Property;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IVigilianceProperty : IProperty<float>, ILoadFromConfigProperty {
		
	}
	public class Vigiliance: AbstractLoadFromConfigProperty<float>, IVigilianceProperty {

		public override float OnSetBaseValueFromConfig(dynamic value) {
			return value;
		}

		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new VigilianceDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.vigiliance;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
		
		
	}
	
	public class VigilianceDefaultModifier : PropertyDependencyModifier<float> {
		public override float OnModify(float propertyValue) {
			return propertyValue * GetDependency<Rarity>(new PropertyNameInfo(PropertyName.rarity)).InitialValue * 5;
		}
	}
}