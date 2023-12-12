using Runtime.DataFramework.Properties;

namespace Runtime.Spawning.Models.Properties
{
    public interface IMaxSpawnTimer : IProperty<float> {
		
    }
    public class MaxSpawnTimer : Property<float>, IMaxSpawnTimer {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return null;
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.max_spawn_timer;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties() {
            return new[] {new PropertyNameInfo(PropertyName.level_number)};
        }
    }
}