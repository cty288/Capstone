using _02._Scripts.Runtime.Levels.Models.Properties;
using Runtime.DataFramework.Properties.CustomProperties;

namespace Runtime.Spawning {
	public class BossPillarEntity : DirectorEntity<BossPillarEntity> {
		
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "BossPillar";

		public override void OnRecycle() {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			base.OnEntityRegisterAdditionalProperties();
			RegisterInitialProperty<ISpawnBossCost>(new SpawnBossCost());
		}
	}
}