using Runtime.DataFramework.Properties;

namespace Runtime.GameResources.Model.Properties {

	public interface IMaxStack : IProperty<int>, ILoadFromConfigProperty {
		
	}
	public class MaxStack : AbstractLoadFromConfigProperty<int>, IMaxStack {
		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.max_stack;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}