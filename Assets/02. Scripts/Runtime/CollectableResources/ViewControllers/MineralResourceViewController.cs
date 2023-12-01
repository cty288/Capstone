using _02._Scripts.Runtime.CollectableResources.Model;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;

namespace _02._Scripts.Runtime.CollectableResources.ViewControllers {
	public class MineralEntity : CollectableResourceEntity<MineralEntity> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "Mineral";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnRecycle() {
			
		}

		protected override void OnInitModifiers(int level, int rarity = 1) {
			
		}
	}
	public class MineralResourceViewController : CollectableResourceViewController<MineralEntity> {
		

		protected override void OnBindEntityProperty() {
			
		}
	}
}