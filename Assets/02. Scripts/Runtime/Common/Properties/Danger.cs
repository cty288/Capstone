using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Property;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IDangerProperty : IProperty<int>, ILoadFromConfigProperty {
		
	}
	public class Danger: AbstractLoadFromConfigProperty<int>, IDangerProperty {

		public override int OnSetBaseValueFromConfig(dynamic value) {
			return value;
		}

		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return new DangerDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.danger;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
		
		
	}
	
	public class DangerDefaultModifier : PropertyDependencyModifier<int> {
		public override int OnModify(int propertyValue) {
			return propertyValue * GetDependency<Rarity>(new PropertyNameInfo(PropertyName.rarity)).InitialValue * 5;
		}
	}
}