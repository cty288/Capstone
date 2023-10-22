using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IGroundDrag : IProperty<float>, ILoadFromConfigProperty { }

	public class GroundDrag: AbstractLoadFromConfigProperty<float>, IGroundDrag {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.ground_drag;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}