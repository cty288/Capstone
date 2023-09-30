using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface ISlideForce : IProperty<float>, ILoadFromConfigProperty { }

	public class SlideForce: AbstractLoadFromConfigProperty<float>, ISlideForce {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.slide_force;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}