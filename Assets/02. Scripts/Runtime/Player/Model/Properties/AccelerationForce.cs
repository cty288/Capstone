using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IAccelerationForce : IProperty<float>, ILoadFromConfigProperty { }

	public class AccelerationForce: AbstractLoadFromConfigProperty<float>, IAccelerationForce {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.acceleration_force;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}