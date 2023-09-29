using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IAdditionalGravity : IProperty<float>, ILoadFromConfigProperty { }

	public class AdditionalGravity: AbstractLoadFromConfigProperty<float>, IAdditionalGravity {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.additional_gravity;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}