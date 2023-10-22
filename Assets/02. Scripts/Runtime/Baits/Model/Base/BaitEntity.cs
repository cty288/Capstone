using _02._Scripts.Runtime.Baits.Model.Property;
using MikroFramework.BindableProperty;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.Enemies.Model.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Base;

namespace _02._Scripts.Runtime.Baits.Model.Base {

	public enum BaitStatus {
		None,
		Baiting,
		Deactivated,
	}
	
	public interface IBaitEntity : IResourceEntity, IHaveCustomProperties, IHaveTags {
		public BindableProperty<float> GetVigiliance();
		public BindableList<TasteType> GetTaste();
		
		public BaitStatus BaitStatus { get; set; }
	}
	public class BaitEntity :  ResourceEntity<BaitEntity>, IBaitEntity {
		[field: ES3Serializable] public override string EntityName { get; set; } = "Bait";
		protected IVigilianceProperty vigilianceProperty;
		protected ITasteProperty tasteProperty;
		
		protected override ConfigTable GetConfigTable() {
			return null;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnAwake() {
			base.OnAwake();
			vigilianceProperty = GetProperty<IVigilianceProperty>();
			tasteProperty = GetProperty<ITasteProperty>();
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			base.OnEntityRegisterAdditionalProperties();
			RegisterInitialProperty<IVigilianceProperty>(new Vigiliance());
			RegisterInitialProperty<ITasteProperty>(new Taste());
		}

		

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		protected override string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName) {
			return originalDisplayName;
		}

		public override ResourceCategory GetResourceCategory() {
			return ResourceCategory.Bait;
		}

		public override string OnGroundVCPrefabName { get; } = "Bait";

		public override string DeployedVCPrefabName { get; } = "Bait_Deployed";
		public BindableProperty<float> GetVigiliance() {
			return this.vigilianceProperty.RealValue;
		}

		public BindableList<TasteType> GetTaste() {
			return this.tasteProperty.RealValues;
		}

		public override void OnRecycle() {
			BaitStatus = BaitStatus.None;
		}

		public override string GetDisplayName() {
			string displayName = Localization.Get($"{EntityName}_name");
			string statusText = "";
			if (BaitStatus == BaitStatus.Baiting) {
				statusText = Localization.Get("BAIT_DEPLOY_STATUS_BAITING");
			}else if (BaitStatus == BaitStatus.Deactivated) {
				statusText = Localization.Get("BAIT_DEPLOY_STATUS_DEACTIVE");
			}
			if (string.IsNullOrEmpty(statusText)) {
				return displayName;
			}
			return $"{displayName} ({statusText})";
		}
		
		

		public BaitStatus BaitStatus { get; set; }
	} 
}