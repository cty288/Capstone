using Runtime.DataFramework.Entities;
using Runtime.RawMaterials.Model.Builder;
using Runtime.RawMaterials.Model.Instances;
using UnityEngine;

namespace Runtime.RawMaterials.ViewControllers.Instances {
	
	/// <summary>
	/// You can use ths class to quickly create a common raw material VC. <br />
	/// In this case, your entity data created from the config sheet will be a <see cref="CommonRawMaterialEntity"/>
	/// </summary>
	public class CommonRawMaterialViewController : AbstractRawMaterialViewController<CommonRawMaterialEntity> {

		[SerializeField] 
		private string entityName;
		
		protected override void OnEntityStart() {
			
		}

		protected override void OnBindEntityProperty() {
			
		}
 
		protected override IEntity OnInitResourceEntity(RawMaterialBuilder<CommonRawMaterialEntity> builder) {
			return builder.OverrideName(entityName).FromConfig().Build();
		}
	}
}