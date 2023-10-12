using Runtime.DataFramework.Properties;

namespace Runtime.Enemies.Model.Properties {
	public interface ILevelNumberProperty : IProperty<int> {
		
	}
	public class LevelNumber: IndependentProperty<int>, ILevelNumberProperty {
		

		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.level_number;
		}
	}
}