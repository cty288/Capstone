﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Player;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Weapons.ViewControllers;
using UnityEngine;

namespace Runtime.GameResources.ViewControllers {
	
	[Serializable]
	public class AnimLayerInfo {
		public string LayerName = "NoItem";
		public float LayerWeight = 1;
	}
	
	public interface IInHandResourceViewController : IResourceViewController {
		void OnStartHold(GameObject ownerGameObject);
		
		void OnStopHold();

		void OnItemStartUse();

		void OnItemStopUse();

		void OnItemUse();

		void OnItemScopePressed();
		
		void OnItemScopeReleased();
		
		Vector3 InHandLocalPosition { get; }
		
		Vector3 InHandLocalRotation { get; }
		
		Vector3 InHandLocalScale { get; }
		
		
		public List<AnimLayerInfo> AnimLayerInfos { get; }
		
		public List<string> PlayerAnimStateToWaitWhenStopHold { get; }

	}

	[Serializable]
	public struct PickableResourceLayerSettings {
		public bool includeChildren;
		public GameObject ownerGameObject;
		
		[HideInInspector]
		public LayerMask originalLayer;
	}
	
	/// <summary>
	/// For both in hand and on ground
	/// </summary>
	/// <typeparam name="T"></typeparam>
	
	public abstract class AbstractPickableInHandResourceViewController<T> : 
		AbstractPickableResourceViewController<T>, IInHandResourceViewController
		where T : class, IResourceEntity, new() {
		
		//private LayerMask originalLayer;
		protected bool isHolding = false;
		protected Rigidbody rigidbody;
		protected GameObject ownerGameObject = null;
		protected float originalAutoRemovalTimeWhenNoAbsorb;
		protected IInventorySystem inventorySystem;


		

		[field: Header("In Hand Settings")]
		[field: SerializeField]
		public Vector3 InHandLocalPosition { get; protected set; } = Vector3.zero;

		[field: SerializeField] 
		public Vector3 InHandLocalRotation { get; protected set; } = Vector3.zero;

		[field: SerializeField] 
		private PickableResourceLayerSettings[] objectToChangeLayerInHand;

		protected Dictionary<GameObject, PickableResourceLayerSettings> objectToChangeLayerInHandDict =
			new Dictionary<GameObject, PickableResourceLayerSettings>();

		[field: SerializeField]
		public Vector3 InHandLocalScale { get; protected set; } = Vector3.one;

		[field: SerializeField]
		public List<AnimLayerInfo> AnimLayerInfos { get; protected set; }

		[field: SerializeField] 
		[field: Tooltip("When the player stops holding the item, the player animator will wait for these states to finish before switching to the next item")]
		public List<string> PlayerAnimStateToWaitWhenStopHold { get; protected set; }

		private Vector3 originalLocalScale;
		
		[Header("Cross hairs")] [SerializeField]
		private string crossHairPrefabName;
		[CanBeNull]
		protected ICrossHairViewController crossHairViewController;
		
		protected LayerMask pickableLayerMask;

		protected override void Awake() {
			base.Awake();
			//originalLayer = gameObject.layer;
			//selfColliders = new Dictionary<Collider, bool>();
			rigidbody = GetComponent<Rigidbody>();
			originalLocalScale = transform.localScale;
			pickableLayerMask = LayerMask.GetMask("PickableResource");
			inventorySystem = this.GetSystem<IInventorySystem>();
			InitObjectsToChangeLayerInHand();
			originalAutoRemovalTimeWhenNoAbsorb = autoRecycleTimeWhenNoAbsorb;
		}

		protected override void Update() {
			base.Update();
			//update in hand local tr
			if (isHolding) {
				transform.localPosition = InHandLocalPosition;
				transform.localEulerAngles = InHandLocalRotation;
				transform.localScale = InHandLocalScale;
			}
		}

		protected void LockCanSwitchInventory(bool isLock) {
			
		}

		private void InitObjectsToChangeLayerInHand() {
			if (objectToChangeLayerInHand == null) {
				return;
			}

			foreach (var layerSetting in objectToChangeLayerInHand) {
				if (layerSetting.ownerGameObject == null) {
					continue;
				}
				//get all children
				if (layerSetting.includeChildren) {
					var children = layerSetting.ownerGameObject.GetComponentsInChildren<Transform>(true);
					foreach (var child in children) {
						if (child == layerSetting.ownerGameObject.transform) {
							continue;
						}
						var o = child.gameObject;
						objectToChangeLayerInHandDict.Add(o, new PickableResourceLayerSettings() {
							originalLayer = o.layer,
							ownerGameObject = layerSetting.ownerGameObject
						});
					}
				}
				
				objectToChangeLayerInHandDict.Add(layerSetting.ownerGameObject, new PickableResourceLayerSettings() {
					originalLayer = layerSetting.ownerGameObject.layer,
					ownerGameObject = layerSetting.ownerGameObject
				});
			}
		}

		protected override bool CanAutoRemoveEntityWhenLevelEnd => !(isHolding || isAbsorbing || isAbsorbWaiting);

		protected override void OnStartAbsorb() {
			SwitchToPickableLayer();
		}
		
		protected void SwitchToPickableLayer() {
			foreach (GameObject o in objectToChangeLayerInHandDict.Keys) {
				o.layer = LayerMask.NameToLayer("PickableResource");
			}
		}
		
		protected void SwitchToOriginalLayer() {
			foreach (GameObject o in objectToChangeLayerInHandDict.Keys) {
				o.layer = objectToChangeLayerInHandDict[o].originalLayer;
			}
		}

		public override void OnRecycled() {
			base.OnRecycled();
			transform.localScale = originalLocalScale;
			inventorySystem.ReleaseLockSwitch(this);
		}
		
		

		protected override void OnReadyToRecycle() {
			base.OnReadyToRecycle();
						
			SwitchToOriginalLayer();
			isHolding = false;
			rigidbody.isKinematic = false;
			this.ownerGameObject = null;
			autoRecycleTimeWhenNoAbsorb = originalAutoRemovalTimeWhenNoAbsorb;
		}


		protected override void HandleAbsorb(GameObject player, PlayerInteractiveZone zone) {
			if (isHolding) {
				return;
			}
			//SwitchToPickableLayer();
			base.HandleAbsorb(player, zone);
		}

		
		public virtual void OnStartHold(GameObject ownerGameObject) {
			/*this.SendEvent<PlayerSwitchAnimEvent>(new PlayerSwitchAnimEvent()
			{
				weight = 1,
				entity = BoundEntity
			});*/
			isHolding = true;
			rigidbody.isKinematic = true;
			foreach (Collider selfCollider in selfColliders.Keys) {
				selfCollider.isTrigger = true;
			}

			
			autoRecycleTimeWhenNoAbsorb = -1;
			this.ownerGameObject = ownerGameObject;
			SwitchToPickableLayer();
			OnUnPointByCrosshair();

			crossHairViewController = Crosshair.Singleton.SpawnCrossHair(crossHairPrefabName);
			if (crossHairViewController != null) {
				crossHairViewController.OnStart(BoundEntity, gameObject);
			}
		}
		
		public virtual void OnStopHold()
		{
			/*this.SendEvent<PlayerSwitchAnimEvent>(new PlayerSwitchAnimEvent()
			{
				weight = 0,
				entity = BoundEntity
			});*/
			
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

		public abstract void OnItemScopeReleased();

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