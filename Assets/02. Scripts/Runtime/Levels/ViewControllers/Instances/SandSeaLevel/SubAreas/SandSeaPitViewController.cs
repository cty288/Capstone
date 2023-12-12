using _02._Scripts.Runtime.Levels.Models;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances {

	public class SandSeaPitEntity : SubAreaLevelEntity<SandSeaPitEntity> {
		[field: ES3Serializable] public override string EntityName { get; set; } = "SandSeaPitEntity";

		public override void OnRecycle() {
			base.OnRecycle();
		}
        
		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}
	}
	public class SandSeaPitViewController : SubAreaLevelViewController<SandSeaPitEntity> {
		protected override ISubAreaLevelEntity OnInitSubLevelEntity(SubAreaLevelBuilder<SandSeaPitEntity> builder)
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