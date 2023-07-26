using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Property;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IAttackRangeProperty : IProperty<float>, ILoadFromConfigProperty{}
	public class AttackRange: AbstractLoadFromConfigProperty<float>, IAttackRangeProperty {
		protected override IPropertyBase[] GetChildProperties() {
			return null;
		}

		public override float OnSetBaseValueFromConfig(dynamic value) {
			return value;
		}

		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new VigilianceDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.attack_range;
		}

		public override PropertyNameInfo[] GetDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
		
		
	}
	
	public class AttackRangeDefaultModifier : PropertyDependencyModifier<float> {
		public override float OnModify(float propertyValue) {
			return propertyValue * GetDependency<Rarity>(new PropertyNameInfo(PropertyName.rarity)).InitialValue * 5;
		}
	}
}