using Runtime.DataFramework.Properties;

namespace Runtime.Spawning.Models.Properties
{
    public interface IMaxEliteEnemies : IProperty<int> {
		
    }
    public class MaxEliteEnemies : Property<int>, IMaxEliteEnemies {
        protected override IPropertyDependencyModifier<int> GetDefautModifier() {
            return null;
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.max_elite_enemies;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties() {
            return new[] {new PropertyNameInfo(PropertyName.level_number)};
        }
    }
}