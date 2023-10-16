using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.TestOnly;

namespace _02._Scripts.Runtime.Baits.Model.Property {
	public class Vigiliance: AbstractLoadFromConfigProperty<float>, IVigilianceProperty {
		protected override PropertyName GetPropertyName() {
			return PropertyName.vigiliance;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity), new PropertyNameInfo(PropertyName.level_number)};
		}
		
		
	}
}