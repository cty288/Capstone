using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Scraps.Model.Base;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;
using Runtime.RawMaterials.Model.Builder;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.InputSystem;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace _02._Scripts.Runtime.Scraps.ViewControllers {
	public interface IScrapViewController : IResourceViewController, IHaveSpawnSizeCollider {
		public void Freeze();
	}
	public class ScrapViewController : AbstractPickableResourceViewController<ScrapEntity>, IScrapViewController {
		private IScrapModel entityModel;
		[SerializeField] private string entityName;
		private Rigidbody rb;

		
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = true;

		protected override void Awake() {
			base.Awake();
			entityModel = this.GetModel<IScrapModel>();
			rb = GetComponent<Rigidbody>();
			
		}

		protected override void OnBindEntityProperty() {
			
		}

		protected override void OnStartAbsorb() {
			
		}
		protected override (InputAction, string, string) GetInteractHintInfo() {
			var b = base.GetInteractHintInfo();
			
			if (inventorySystem.CanPlaceItem(BoundEntity)) {
				return (b.Item1, Localization.Get("SCRAP_INTERACT"), b.Item3);
			}else {
				return (b.Item1, Localization.Get("SCRAP_INTERACT_FULL_INV"), b.Item3);
			}
		}
		public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity, bool addToModelWhenBuilt = true) {
			if (entityModel == null) {
				entityModel = this.GetModel<IScrapModel>();
			}

			var builder = entityModel.GetBuilder(addToModelWhenBuilt);
			if (setRarity) {
				builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), rarity);
			}
			return  builder.OverrideName(entityName).FromConfig().Build();
		}


		protected override void OnPlayerPressInteract() {
			base.OnPlayerPressInteract();
			if (inventorySystem.AddItem(BoundEntity)) {
				RecycleToCache();
			}
		}

		protected override void OnEntityStart() {
			base.OnEntityStart();
			/*rb.isKinematic = !BoundEntity.PickedBefore;
			rb.constraints = BoundEntity.PickedBefore ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;*/
		}

		public override void OnRecycled() {
			base.OnRecycled();
			rb.isKinematic = false;
			rb.constraints = RigidbodyConstraints.None;
			transform.rotation = Quaternion.identity;
		}

		public void Freeze() {
			rb.isKinematic = true;
			rb.constraints = RigidbodyConstraints.FreezeAll;
		}

		public BoxCollider SpawnSizeCollider {
			get {
				return 	transform.Find("SelfSizeCollider").GetComponent<BoxCollider>();
			}
		}
	}
}