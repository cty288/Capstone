using _02._Scripts.Runtime.Skills.Model.Properties;

namespace Runtime.GameResources.Model.Base {
	public interface IBuildableResourceEntity : IResourceEntity {
		public PurchaseCostInfo GetPurchaseCost();
		
		public int GetMaxRarity();
		
		public int GetMinRarity();
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

		protected override void OnRegisterProperties() {
			base.OnRegisterProperties();
			RegisterInitialProperty<IPurchaseCost>(new PurchaseCost());
		}

		public abstract int GetMaxRarity();
		public abstract int GetMinRarity();
	}
}