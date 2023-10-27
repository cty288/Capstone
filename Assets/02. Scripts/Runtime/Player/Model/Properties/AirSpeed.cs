using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	
	public interface IAirSpeedProperty : IProperty<float>, ILoadFromConfigProperty { }

	public class AirSpeed: AbstractLoadFromConfigProperty<float>, IAirSpeedProperty {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.air_speed;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}