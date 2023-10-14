using Runtime.DataFramework.Properties;

namespace Runtime.Spawning.Models.Properties
{
    public interface ISpawnTimer : IProperty<float> {
		
    }
    public class SpawnTimer : Property<float>, ISpawnTimer {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return null;
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.spawn_timer;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties() {
            return new[] {new PropertyNameInfo(PropertyName.level_number)};
        }
    }
}