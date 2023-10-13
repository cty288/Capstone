using _02._Scripts.Runtime.Baits.Model.Property;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.Enemies.Model.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Base;

namespace _02._Scripts.Runtime.Baits.Model.Base {
	
	public interface IBaitEntity : IResourceEntity, IHaveCustomProperties, IHaveTags {
		
	}
	public class BaitEntity :  ResourceEntity<BaitEntity>, IBaitEntity {
		[field: ES3Serializable] public override string EntityName { get; set; } = "Bait";
		
		protected override ConfigTable GetConfigTable() {
			return null;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			base.OnEntityRegisterAdditionalProperties();
			RegisterInitialProperty<IVigilianceProperty>(new Vigiliance());
			RegisterInitialProperty<ITasteProperty>(new Taste());
		}

		public override void OnRecycle() {
			
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return null;
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
	} 
}