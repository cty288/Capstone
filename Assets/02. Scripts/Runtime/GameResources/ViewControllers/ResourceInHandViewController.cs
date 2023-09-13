using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Player;
using UnityEngine;

namespace Runtime.GameResources.ViewControllers {
	public interface IInHandResourceViewController : IResourceViewController {
		void OnStartHold();
		
		void OnStopHold();
	}
	
	
	/// <summary>
	/// For both in hand and on ground
	/// </summary>
	/// <typeparam name="T"></typeparam>
	
	public abstract class AbstractPickableInHandResourceViewController<T> : 
		AbstractPickableResourceViewController<T>, IInHandResourceViewController
		where T : class, IResourceEntity, new() {
		protected Collider[] selfColliders;
		private LayerMask originalLayer;
		protected bool isHolding = false;
		protected Rigidbody rigidbody;

		protected override void Awake() {
			base.Awake();
			originalLayer = gameObject.layer;
			selfColliders = GetComponents<Collider>();
			rigidbody = GetComponent<Rigidbody>();
		}

		protected override void OnStartAbsorb() {
			foreach (Collider selfCollider in selfColliders) {
				selfCollider.isTrigger = true;
			}

			gameObject.layer = LayerMask.NameToLayer("PickableResource");
		}

		public override void OnRecycled() {
			base.OnRecycled();
			foreach (Collider selfCollider in selfColliders) {
				selfCollider.isTrigger = false;
			}
			gameObject.layer = originalLayer;
			isHolding = false;
			rigidbody.isKinematic = false;
		}

		protected override void HandleAbsorb(GameObject player, PlayerInteractiveZone zone) {
			if (isHolding) {
				return;
			}
			base.HandleAbsorb(player, zone);
		}

		public void OnStartHold() {
			isHolding = true;
			rigidbody.isKinematic = true;
			foreach (Collider selfCollider in selfColliders) {
				selfCollider.isTrigger = true;
			}
			gameObject.layer = LayerMask.NameToLayer("PickableResource");
			OnUnPointByCrosshair();
		}

		public void OnStopHold() {
			isHolding = false;
			rigidbody.isKinematic = false;
			//gameObject.layer = originalLayer;
			RecycleToCache();
		}

		public override void OnPointByCrosshair() {
			if (isHolding) {
				return;
			}
			base.OnPointByCrosshair();
		}


	}
}