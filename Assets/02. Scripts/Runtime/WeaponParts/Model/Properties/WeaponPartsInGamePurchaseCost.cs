using _02._Scripts.Runtime.Skills.Model.Properties;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.WeaponParts.Model.Properties {
	public interface IWeaponPartsInGamePurchaseCostProperty : ILeveledProperty<int>, IDictionaryProperty<int,int>, ILoadFromConfigProperty {
		
	}
	
	
	public class WeaponPartsInGamePurchaseCostProperty : LeveledProperty<int>, IWeaponPartsInGamePurchaseCostProperty {
		protected override PropertyName GetPropertyName() {
			return PropertyName.parts_ingame_purchase_cost;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
		
	}
}