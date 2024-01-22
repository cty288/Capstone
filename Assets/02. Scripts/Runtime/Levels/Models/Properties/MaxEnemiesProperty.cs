using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.Levels.Models.Properties {
	public interface IMaxEnemiesProperty : IProperty<int> {
		
	}
	public class MaxEnemiesProperty : Property<int>, IMaxEnemiesProperty {
		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.max_enemies;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
	}
}