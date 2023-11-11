using _02._Scripts.Runtime.CollectableResources.Model;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;

namespace _02._Scripts.Runtime.CollectableResources.ViewControllers {
	public class PlantEntity : CollectableResourceEntity<PlantEntity> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "Plant";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnRecycle() {
			
		}

		protected override void OnInitModifiers(int level, int rarity = 1) {
			
		}
	}
	public class PlantResourceViewController : CollectableResourceViewController<PlantEntity> {
		protected override void OnEntityStart() {
			
		}

		protected override void OnBindEntityProperty() {
			
		}
	}
}