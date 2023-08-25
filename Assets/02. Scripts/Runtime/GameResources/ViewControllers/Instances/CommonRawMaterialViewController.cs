using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Builder;
using Runtime.GameResources.Model.Instances;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace Runtime.GameResources.ViewControllers.Instances.Instances {
	
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

		protected override IEntity OnInitEnemyEntity(RawMaterialBuilder<CommonRawMaterialEntity> builder) {
			return builder.OverrideName(entityName).FromConfig().Build();
		}
	}
}