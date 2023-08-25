using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;

namespace Runtime.GameResources.Model.Instances {
	public class CommonRawMaterialEntity : RawMaterialEntity<CommonRawMaterialEntity> {
		[field: ES3Serializable]
		public override string EntityName { get; set; }
		
		public CommonRawMaterialEntity():base(){}
		
		
		public override void OnRecycle() {
			
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}
	}
}