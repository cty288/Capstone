using Runtime.DataFramework.Properties;

namespace Runtime.Spawning.Models.Properties
{
    public interface IPackSpawnTimer : IProperty<float> {
		
    }
    public class PackSpawnTimer : Property<float>, IPackSpawnTimer {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return null;
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.pack_spawn_timer;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties() {
            return new[] {new PropertyNameInfo(PropertyName.rarity)};
        }
    }
}