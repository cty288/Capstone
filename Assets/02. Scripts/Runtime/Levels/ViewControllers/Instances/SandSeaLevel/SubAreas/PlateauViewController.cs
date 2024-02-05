using _02._Scripts.Runtime.Levels.Models;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances {

	public class PlateauEntity : SubAreaLevelEntity<PlateauEntity> {
		[field: ES3Serializable] public override string EntityName { get; set; } = "PlateauEntity";

		public override void OnRecycle() {
			base.OnRecycle();
		}
        
		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}
	}
	public class PlateauViewController : SubAreaLevelViewController<PlateauEntity> {
		protected override ISubAreaLevelEntity OnInitSubLevelEntity(SubAreaLevelBuilder<PlateauEntity> builder)
		{
			return builder
				.Build();
		}

		protected override void OnBindEntityProperty()
		{
		}
	}
}