using Runtime.DataFramework.Properties;

namespace Runtime.Spawning.Models.Properties
{
    public interface IMaxDirectorEnemies : IProperty<int> {
		
    }
    public class MaxDirectorEnemies : Property<int>, IMaxDirectorEnemies {
        protected override IPropertyDependencyModifier<int> GetDefautModifier() {
            return null;
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.max_director_enemies;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties() {
            return new[] {new PropertyNameInfo(PropertyName.level_number)};
        }
    }
}