using _02._Scripts.Runtime.Skills.Model.Properties;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.WeaponParts.Model.Properties {
	public interface IWeaponPartsUpgradeCostProperty : ILeveledProperty<int>, IDictionaryProperty<int,int>, ILoadFromConfigProperty {
		
	}
	
	
	public class WeaponPartsUpgradeCostProperty : LeveledProperty<int>, IWeaponPartsUpgradeCostProperty {
		protected override PropertyName GetPropertyName() {
			return PropertyName.parts_upgrade_cost;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
		
	}
}