using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	
	public interface IAirDrag : IProperty<float>, ILoadFromConfigProperty { }

	public class AirDrag: AbstractLoadFromConfigProperty<float>, IAirDrag {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.air_drag;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}