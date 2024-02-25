using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.WeaponParts.Model.Properties {
	public interface IBuildType : IProperty<CurrencyType>, ILoadFromConfigProperty { }
	
	public class BuildType : AbstractLoadFromConfigProperty<CurrencyType>, IBuildType {
		protected override PropertyName GetPropertyName() {
			return PropertyName.build_type;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}

}