using Runtime.DataFramework.Properties;

namespace Runtime.Spawning.Models.Properties
{
    public interface ICreditsPerSecond : IProperty<float> {
		
    }
    public class CreditsPerSecond : Property<float>, ICreditsPerSecond {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return null;
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.credits_per_second;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties() {
            return new[] {new PropertyNameInfo(PropertyName.level_number)};
        }
    }
}