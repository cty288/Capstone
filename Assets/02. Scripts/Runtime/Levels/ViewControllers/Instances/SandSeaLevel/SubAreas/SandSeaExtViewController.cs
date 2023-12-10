using _02._Scripts.Runtime.Levels.Models;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances {

	public class SandSeaExtEntity : SubAreaLevelEntity<SandSeaExtEntity> {
		[field: ES3Serializable] public override string EntityName { get; set; } = "SandSeaExtEntity";

		public override void OnRecycle() {
			base.OnRecycle();
		}
        
		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}
	}
	public class SandSeaExtViewController : SubAreaLevelViewController<SandSeaExtEntity> {
		protected override ISubAreaLevelEntity OnInitSubLevelEntity(SubAreaLevelBuilder<SandSeaExtEntity> builder)
		{
			return builder
				.Build();
		}

		protected override void OnEntityStart()
		{
		}

		protected override void OnBindEntityProperty()
		{
		}
	}
}