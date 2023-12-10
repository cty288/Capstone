using _02._Scripts.Runtime.Levels.Models;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances {

	public class CactusDesertEntity : SubAreaLevelEntity<CactusDesertEntity> {
		[field: ES3Serializable] public override string EntityName { get; set; } = "CactusDesertEntity";

		public override void OnRecycle() {
			base.OnRecycle();
		}
        
		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}
	}
	public class CactusDesertViewController : SubAreaLevelViewController<CactusDesertEntity> {
		protected override ISubAreaLevelEntity OnInitSubLevelEntity(SubAreaLevelBuilder<CactusDesertEntity> builder)
		{
			print($"building Cactus Desert VC entity");

			return builder
				.Build();
		}

		protected override void OnEntityStart()
		{
			print($"{BoundEntity.EntityName} is starting in Cactus Desert VC");
		}

		protected override void OnBindEntityProperty()
		{
		}
	}
}