namespace Runtime.DataFramework.Properties {
	public interface IDangerProperty : IProperty<int>, ILoadFromConfigProperty {
		
	}
	public class Danger: AbstractLoadFromConfigProperty<int>, IDangerProperty {
		

		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.danger;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
		
		
	}
}