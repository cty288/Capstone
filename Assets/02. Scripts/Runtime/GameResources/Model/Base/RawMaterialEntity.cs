using Runtime.Utilities.ConfigSheet;

namespace Runtime.GameResources.Model.Base {
	
	public interface IRawMaterialEntity : IResourceEntity {
		
	}
	public abstract class RawMaterialEntity<T> : ResourceEntity<T>, IRawMaterialEntity where T : ResourceEntity<T>, new() {
		protected override ConfigTable GetConfigTable() {
			
			return ConfigDatas.Singleton.RawMaterialEntityConfigTable;
		}
	}
}