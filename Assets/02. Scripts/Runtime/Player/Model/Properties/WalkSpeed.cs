using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IWalkSpeed : IProperty<float>, ILoadFromConfigProperty { }

	public class WalkSpeed: AbstractLoadFromConfigProperty<float>, IWalkSpeed {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.walk_speed;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}