using Runtime.DataFramework.Properties;

namespace Runtime.Enemies.Model.Properties {
	public interface ISpawnCostProperty : IProperty<float>, ILoadFromConfigProperty {
		
	}
	public class SpawnCost : AbstractLoadFromConfigProperty<float>, ISpawnCostProperty {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.spawn_cost;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}