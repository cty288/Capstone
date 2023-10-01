using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IMaxSlideTime : IProperty<float>, ILoadFromConfigProperty { }

	public class MaxSlideTime: AbstractLoadFromConfigProperty<float>, IMaxSlideTime {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.max_slide_time;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}