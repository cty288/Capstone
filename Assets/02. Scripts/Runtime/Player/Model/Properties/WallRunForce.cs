using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IWallRunForce : IProperty<float>, ILoadFromConfigProperty { }

	public class WallRunForce: AbstractLoadFromConfigProperty<float>, IWallRunForce {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.wall_run_force;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}