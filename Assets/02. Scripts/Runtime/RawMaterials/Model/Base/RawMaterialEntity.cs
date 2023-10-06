using Polyglot;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Model.Properties.BaitAdjectives;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.RawMaterials.Model.Base {
	
	public interface IRawMaterialEntity : IResourceEntity {
		public IBaitAdjectives GetBaitAdjectivesProperty();
	}
	public abstract class RawMaterialEntity<T> : ResourceEntity<T>, IRawMaterialEntity where T : ResourceEntity<T>, new() {
		private IBaitAdjectives baitAdjectivesProperty;
		

		public override void OnAwake() {
			base.OnAwake();
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

		protected override string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName) {
			return "???";
		}

		[field: ES3Serializable] public override string OnGroundVCPrefabName { get; } = "RawMaterial_OnGround";
	}
}