using Polyglot;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Model.Properties;
using Runtime.GameResources.Model.Properties.BaitAdjectives;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.RawMaterials.Model.Base {
	
	public interface IRawMaterialEntity : IResourceEntity {
		public IBaitAdjectives GetBaitAdjectivesProperty();
	}

	public interface IHaveExpResourceEntity : IResourceEntity {
		public IExp GetExpProperty();
	}
	public abstract class RawMaterialEntity<T> : ResourceEntity<T>, IRawMaterialEntity, IHaveExpResourceEntity
		where T : ResourceEntity<T>, new() {
		
		private IBaitAdjectives baitAdjectivesProperty;
		private IExp expProperty;

		public override void OnResourceAwake() {
			base.OnResourceAwake();
			baitAdjectivesProperty = GetProperty<IBaitAdjectives>();
			expProperty = GetProperty<IExp>();
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			base.OnEntityRegisterAdditionalProperties();
			RegisterInitialProperty<IBaitAdjectives>(new BaitAdjectives());
			RegisterInitialProperty<IExp>(new ExpProperty());
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
		public IExp GetExpProperty() {
			return expProperty;
		}
	}
}