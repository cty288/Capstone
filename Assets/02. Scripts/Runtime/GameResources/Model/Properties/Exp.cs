using Runtime.DataFramework.Properties;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;
namespace Runtime.GameResources.Model.Properties {
	
	public interface IExp : IProperty<int>, ILoadFromConfigProperty {
		
	}
	
	
	public class ExpProperty : AbstractLoadFromConfigProperty<int>, IExp {
		protected override PropertyName GetPropertyName() {
			return PropertyName.exp;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}