using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IHealthRecoverSpeed : IProperty<float>, ILoadFromConfigProperty { }
	public class HealthRecoverSpeed : AbstractLoadFromConfigProperty<float>, IHealthRecoverSpeed {
		protected override PropertyName GetPropertyName() {
			return PropertyName.health_recover_speed;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}