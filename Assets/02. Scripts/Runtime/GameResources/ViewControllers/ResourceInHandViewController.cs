using System.Collections.Generic;
using JetBrains.Annotations;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Player;
using Runtime.Weapons.ViewControllers;
using UnityEngine;

namespace Runtime.GameResources.ViewControllers {
	public interface IInHandResourceViewController : IResourceViewController {
		void OnStartHold(GameObject ownerGameObject);
		
		void OnStopHold();

		void OnItemStartUse();

		void OnItemStopUse();

		void OnItemUse();

		void OnItemScopePressed();
		
		Vector3 InHandLocalPosition { get; }
		
		Vector3 InHandLocalRotation { get; }
		
		Vector3 InHandLocalScale { get; }
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
		
		[field: Header("In Hand Transform")]
		[field: SerializeField]
		public Vector3 InHandLocalPosition { get; protected set; } = Vector3.zero;

		[field: SerializeField] 
		public Vector3 InHandLocalRotation { get; protected set; } = Vector3.zero;
		
		[field: SerializeField]
		public Vector3 InHandLocalScale { get; protected set; } = Vector3.one;
		
		[Header("Cross hairs")] [SerializeField]
		private string crossHairPrefabName;
		[CanBeNull]
		protected ICrossHairViewController crossHairViewController;

		protected override void Awake() {
			base.Awake();
			originalLayer = gameObject.layer;
			//selfColliders = new Dictionary<Collider, bool>();
			rigidbody = GetComponent<Rigidbody>();
			
		}

		protected override void OnStartAbsorb() {
			gameObject.layer = LayerMask.NameToLayer("PickableResource");
		}

		public override void OnRecycled() {
			base.OnRecycled();

		}

		protected override void OnReadyToRecycle() {
			base.OnReadyToRecycle();
						
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
				selfCollider.isTrigger = true;
			}

			originalAutoRemovalTimeWhenNoAbsorb = entityAutoRemovalTimeWhenNoAbsorb;
			entityAutoRemovalTimeWhenNoAbsorb = -1;
			this.ownerGameObject = ownerGameObject;
			gameObject.layer = LayerMask.NameToLayer("PickableResource");
			OnUnPointByCrosshair();

			crossHairViewController = Crosshair.Singleton.SpawnCrossHair(crossHairPrefabName);
			if (crossHairViewController != null) {
				crossHairViewController.OnStart(BoundEntity, gameObject);
			}
		}
		
		public virtual void OnStopHold() {
			rigidbody.isKinematic = false;
			foreach (Collider selfCollider in selfColliders.Keys) {
				selfCollider.isTrigger = selfColliders[selfCollider];
			}
			//gameObject.layer = originalLayer;
			if (crossHairViewController != null) {
				crossHairViewController.OnStopHold();
				Crosshair.Singleton.DespawnCrossHair();
			}
			RecycleToCache();
		}

		public abstract void OnItemStartUse();

		public abstract void OnItemStopUse();

		public abstract void OnItemUse();
		public abstract void OnItemScopePressed();
		


		public override void OnPointByCrosshair() {
			if (isHolding) {
				return;
			}
			base.OnPointByCrosshair();
		}

		protected override void OnEntityRecycled(IEntity ent) {
			base.OnEntityRecycled(ent);
			transform.localScale = Vector3.one;
			transform.rotation = Quaternion.identity;
		}
	}
}