using Runtime.DataFramework.Properties;

namespace Runtime.Spawning.Models.Properties
{
    public interface IDirectorCooldown : IProperty<float> {
		
    }
    public class DirectorCooldown : Property<float>, IDirectorCooldown {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return null;
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.director_cooldown;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties() {
            return new[] {new PropertyNameInfo(PropertyName.level_number)};
        }
    }
}