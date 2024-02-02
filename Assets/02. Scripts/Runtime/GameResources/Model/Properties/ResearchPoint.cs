using Runtime.DataFramework.Properties;

namespace Runtime.GameResources.Model.Properties {

	public interface IResearchPoint : IProperty<int>, ILoadFromConfigProperty {
		
	}
	public class ResearchPoint : AbstractLoadFromConfigProperty<int>, IResearchPoint {
		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.research_point;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}