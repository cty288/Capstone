using _02._Scripts.Runtime.Skills.Model.Properties;

namespace Runtime.GameResources.Model.Base {
	public interface IBuildableResourceEntity : IResourceEntity {
		public PurchaseCostInfo GetPurchaseCost();
	}

	public abstract class BuildableResourceEntity<T> : ResourceEntity<T>, IBuildableResourceEntity
		where T : BuildableResourceEntity<T>, new() {
		protected IPurchaseCost PurchaseCostProperty;

		public override void OnResourceAwake() {
			base.OnResourceAwake();
			PurchaseCostProperty = GetProperty<IPurchaseCost>();
		}

		public PurchaseCostInfo GetPurchaseCost() {
			return PurchaseCostProperty.RealValue.Value;
		}
	}
}