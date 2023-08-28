using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Model.Properties.BaitAdjectives;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.RawMaterials.Model.Base {
	
	public interface IRawMaterialEntity : IResourceEntity {
		public IBaitAdjectives GetBaitAdjectivesProperty();
	}
	public abstract class RawMaterialEntity<T> : ResourceEntity<T>, IRawMaterialEntity where T : ResourceEntity<T>, new() {
		private IBaitAdjectives baitAdjectivesProperty;
		protected override void OnEntityStart() {
			base.OnEntityStart();
			baitAdjectivesProperty = GetProperty<IBaitAdjectives>();
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			base.OnEntityRegisterAdditionalProperties();
			RegisterInitialProperty<IBaitAdjectives>(new BaitAdjectives());
		}

		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.RawMaterialEntityConfigTable;
		}
		
		public IBaitAdjectives GetBaitAdjectivesProperty() {
			return baitAdjectivesProperty;
		}
	}
}