using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IJumpForce : IProperty<float>, ILoadFromConfigProperty { }

	public class JumpForce: AbstractLoadFromConfigProperty<float>, IJumpForce {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.jump_force;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}