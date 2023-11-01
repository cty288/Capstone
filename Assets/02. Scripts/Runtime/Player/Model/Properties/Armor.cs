using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IArmorProperty : IProperty<float>, ILoadFromConfigProperty { }

	public class ArmorProperty: AbstractLoadFromConfigProperty<float>, IArmorProperty {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.armor;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}