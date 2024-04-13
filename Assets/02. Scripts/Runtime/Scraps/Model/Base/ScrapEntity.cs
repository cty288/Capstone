using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Model.Properties;
using Runtime.RawMaterials.Model.Base;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Scraps.Model.Base {
	public interface IScrapEntity : IResourceEntity {
		public bool PickedBefore { get; }
	}
	public class ScrapEntity : ResourceEntity<ScrapEntity>, IScrapEntity, IHaveExpResourceEntity {
		
		[field: ES3Serializable]
		public override string EntityName { get; set; }

		[ES3Serializable] private bool pickedBefore;
		
		private IExp expProperty;
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.RawMaterialEntityConfigTable;
		}
		public override void OnResourceAwake() {
			base.OnResourceAwake();
			expProperty = GetProperty<IExp>();
		}
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnRecycle() {
			pickedBefore = false;
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get("Scrap_desc");
		}


		public override string GetDisplayName() {
			return Localization.Get("Scrap_name");
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			base.OnEntityRegisterAdditionalProperties();
			RegisterInitialProperty<IExp>(new ExpProperty());
		}

		protected override string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName) {
			return originalDisplayName;
		}

		public override ResourceCategory GetResourceCategory() {
			return ResourceCategory.Scrap;
		}

		public override string OnGroundVCPrefabName => EntityName;

		//public override string OnGroundVCPrefabName => "Scrap_OnGround";
		
		[field: ES3Serializable]
		public override bool Collectable { get; } = true;
		public override IResourceEntity GetReturnToBaseEntity() {
			return this;
		}

		public override void OnAddedToInventory(string playerUUID) {
			base.OnAddedToInventory(playerUUID);
			pickedBefore = true;
		}

		public IExp GetExpProperty() {
			return expProperty;
		}


		public bool PickedBefore => pickedBefore;
		public override string GetIconName() {
			return base.GetIconName();
		}
	}
}