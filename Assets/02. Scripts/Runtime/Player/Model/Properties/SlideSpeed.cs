using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface ISlideSpeed : IProperty<float>, ILoadFromConfigProperty { }

	public class SlideSpeed: AbstractLoadFromConfigProperty<float>, ISlideSpeed {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.slide_speed;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}