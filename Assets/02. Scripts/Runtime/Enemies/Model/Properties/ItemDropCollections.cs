using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;

namespace Runtime.Enemies.Model.Properties {
	public interface IItemDropCollectionsProperty : IDictionaryProperty<int, ItemDropCollection> {
		
	}
	public class ItemDropCollections : LoadFromConfigDictProperty<int, ItemDropCollection>, IItemDropCollectionsProperty {
		protected override PropertyName GetPropertyName() {
			return PropertyName.item_drop_collections;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}