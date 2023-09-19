using System.Collections.Generic;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Player;
using UnityEngine;

namespace Runtime.GameResources.ViewControllers {
	public interface IInHandResourceViewController : IResourceViewController {
		void OnStartHold(GameObject ownerGameObject);
		
		void OnStopHold();
	}
	
	
	/// <summary>
	/// For both in hand and on ground
	/// </summary>
	/// <typeparam name="T"></typeparam>
	
	public abstract class AbstractPickableInHandResourceViewController<T> : 
		AbstractPickableResourceViewController<T>, IInHandResourceViewController
		where T : class, IResourceEntity, new() {
		
		private LayerMask originalLayer;
		protected bool isHolding = false;
		protected Rigidbody rigidbody;
		protected GameObject ownerGameObject = null;
		protected float originalAutoRemovalTimeWhenNoAbsorb;

		protected override void Awake() {
			base.Awake();
			originalLayer = gameObject.layer;
			selfColliders = new Dictionary<Collider, bool>();
			rigidbody = GetComponent<Rigidbody>();
			
		}

		protected override void OnStartAbsorb() {
			gameObject.layer = LayerMask.NameToLayer("PickableResource");
		}

		public override void OnRecycled() {
			base.OnRecycled();
			
			gameObject.layer = originalLayer;
			isHolding = false;
			rigidbody.isKinematic = false;
			this.ownerGameObject = null;
			entityAutoRemovalTimeWhenNoAbsorb = originalAutoRemovalTimeWhenNoAbsorb;
		}
		
		

		protected override void HandleAbsorb(GameObject player, PlayerInteractiveZone zone) {
			if (isHolding) {
				return;
			}
			gameObject.layer = LayerMask.NameToLayer("PickableResource");
			base.HandleAbsorb(player, zone);
		}

		public virtual void OnStartHold(GameObject ownerGameObject) {
			isHolding = true;
			rigidbody.isKinematic = true;
			foreach (Collider selfCollider in selfColliders.Keys) {
				selfCollider.isTrigger = selfColliders[selfCollider];
			}

			originalAutoRemovalTimeWhenNoAbsorb = entityAutoRemovalTimeWhenNoAbsorb;
			entityAutoRemovalTimeWhenNoAbsorb = -1;
			this.ownerGameObject = ownerGameObject;
			gameObject.layer = LayerMask.NameToLayer("PickableResource");
			OnUnPointByCrosshair();
		}
		
		public virtual void OnStopHold() {
			
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