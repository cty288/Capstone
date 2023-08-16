namespace Runtime.DataFramework.Properties {
	public interface IAttackRangeProperty : IProperty<float>, ILoadFromConfigProperty{}
	public class TestAttackRange: AbstractLoadFromConfigProperty<float>, IAttackRangeProperty {
		public override float OnSetBaseValueFromConfig(dynamic value) {
			return value;
		}

		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new VigilianceDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.attack_range;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
		
		
	}
	
	public class AttackRangeDefaultModifier : PropertyDependencyModifier<float> {
		public override float OnModify(float propertyValue) {
			return propertyValue * GetDependency<Rarity>(new PropertyNameInfo(PropertyName.rarity)).RealValue * 5;
		}
	}
}