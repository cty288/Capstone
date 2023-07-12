using System.Collections.Generic;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IAttackRangeProperty : IProperty<float>{}
	public class AttackRange: Property<float>, IAttackRangeProperty {
		
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new VigilianceDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.attack_range;
		}

		public override PropertyName[] GetDependentProperties() {
			return new[] {PropertyName.rarity};
		}
		
		
	}
	
	public class AttackRangeDefaultModifier : PropertyDependencyModifier<float> {
		public override float OnModify(float propertyValue) {
			return propertyValue * GetDependency<Rarity>().InitialValue * 5;
		}
	}
}