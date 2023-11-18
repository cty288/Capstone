using Runtime.DataFramework.Properties;

namespace Runtime.Player.Properties {
	public interface IArmorRecoverSpeedProperty : IProperty<float>, ILoadFromConfigProperty { }

	public class ArmorRecoverSpeedProperty: AbstractLoadFromConfigProperty<float>, IArmorRecoverSpeedProperty {
		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new EmptyModifier<float>();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.armor_recover_speed;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}