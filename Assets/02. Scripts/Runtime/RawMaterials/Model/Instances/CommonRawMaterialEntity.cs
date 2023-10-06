using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Base;

namespace Runtime.RawMaterials.Model.Instances {
	public class CommonRawMaterialEntity : RawMaterialEntity<CommonRawMaterialEntity> {
		[field: ES3Serializable]
		public override string EntityName { get; set; }
		
		public CommonRawMaterialEntity():base(){}
		protected override void OnInitModifiers(int rarity) {
            
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnRecycle() {
			
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		public override ResourceCategory GetResourceCategory() {
			return ResourceCategory.RawMaterial;
		}
	}
}