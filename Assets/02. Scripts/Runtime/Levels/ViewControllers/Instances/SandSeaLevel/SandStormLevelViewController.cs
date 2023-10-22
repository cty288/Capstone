using _02._Scripts.Runtime.Levels.Models;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances {

	public class SandStormLevelEntity : LevelEntity<SandStormLevelEntity> {
		[field: ES3Serializable] public override string EntityName { get; set; } = "SandStormLevelEntity";

		public override void OnRecycle() {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}
	}
	public class SandStormLevelViewController : NormalLevelViewController<SandStormLevelEntity> {
		protected override void OnEntityStart() {
			
		}

		protected override void OnBindEntityProperty() {
			
		}

		protected override IEntity OnInitLevelEntity(LevelBuilder<SandStormLevelEntity> builder, int levelNumber) {
			return builder
				.Build();
		}
	}
}