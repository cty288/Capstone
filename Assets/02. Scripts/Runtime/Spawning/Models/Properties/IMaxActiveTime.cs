using Runtime.DataFramework.Properties;

namespace Runtime.Spawning.Models.Properties
{
    public interface IMaxActiveTime : IProperty<float> {
		
    }
    
    public class MaxActiveTime : Property<float>, IMaxActiveTime {
        protected override IPropertyDependencyModifier<float> GetDefautModifier() {
            return null;
        }

        protected override PropertyName GetPropertyName() {
            return PropertyName.max_active_time;
        }

        public override PropertyNameInfo[] GetDefaultDependentProperties() {
            return new[] {new PropertyNameInfo(PropertyName.level_number)};
        }
    }
}