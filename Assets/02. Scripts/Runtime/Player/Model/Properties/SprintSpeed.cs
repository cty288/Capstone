using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface ISprintSpeed : IProperty<float>, ILoadFromConfigProperty { }

	public class SprintSpeed: AbstractLoadFromConfigProperty<float>, ISprintSpeed {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.sprint_speed;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}