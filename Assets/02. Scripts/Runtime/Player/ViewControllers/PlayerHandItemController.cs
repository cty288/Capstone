using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Player.Commands;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Newtonsoft.Json;
using Runtime.Controls;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Player;
using Runtime.Utilities;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Weapons;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.ViewControllers;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using Object = UnityEngine.Object;

public class PlayerHandItemController : EntityAttachedViewController<PlayerEntity> {
	private IInventoryModel inventoryModel;
	[SerializeField] private Transform leftHandTr;
	[SerializeField] private Transform rightHandTr;
	[SerializeField] private float deployDistance = 10f;
	[SerializeField] private WeaponSway armsHolder;
	private LayerMask deployGroundLayerMask;
	private Animator animator;

	//private Dictionary<HotBarCategory, IInHandResourceViewController> inHandResourceViewControllers =
	//	new Dictionary<HotBarCategory, IInHandResourceViewController>();

	
	private IInHandResourceViewController currentHoldItemViewController = null;
	private (IDeployableResourceViewController, GameObject) currentHoldDeployableItemViewController = (null, null);
	
	private IGamePlayerModel playerModel;
	private DPunkInputs.PlayerActions playerActions;

	private BindableProperty<DeployFailureReason> deployFailureReason =
		new BindableProperty<DeployFailureReason>(DeployFailureReason.NA);
	
	private HotBarCategory currentHand = HotBarCategory.Right;
	
	private List<string> waitingPlayerAnimStatesBeforeNextItem = null;
	private IResourceEntity waitingEntity;
	private HotBarCategory waitingCategory;
	private List<int> waitingActiveLayers;

	//private bool justSwitchItem = false;
	//private List<string> waitingPlayerAnimStatesBeforeNextItem = new List<string>();
	//private 

	//private IInHandResourceViewController rightHandItemViewController = null;
	
	protected override void Awake() {
		base.Awake();
		inventoryModel = this.GetModel<IInventoryModel>();
		playerModel = this.GetModel<IGamePlayerModel>();
		playerActions = ClientInput.Singleton.GetPlayerActions();
		deployFailureReason.RegisterWithInitValue(OnDeployFailureReasonChanged)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		deployGroundLayerMask = LayerMask.GetMask("Default", "Ground", "Wall");
		Debug.Log(JsonConvert.DeserializeObject<Vector2Int>("{\"x\":1,\"y\":2}"));
		animator = transform.Find("CameraFollower/ArmsHolder").GetChild(0).GetComponent<Animator>();
		
	}

	

	private void OnDeployFailureReasonChanged(DeployFailureReason lastReason, DeployFailureReason currentReason) {
		if (currentReason == DeployFailureReason.NA || currentHoldDeployableItemViewController.Item1 == null ||
		    currentHoldItemViewController == null) {
			return;
		}

		if (lastReason != currentReason) {
			(currentHoldItemViewController as IInHandDeployableResourceViewController)?.OnDeployFailureReasonChanged(
				lastReason, currentReason);
			
			currentHoldDeployableItemViewController.Item1.OnCanDeployFailureStateChanged(lastReason, currentReason);
		}
		
	}

	protected override void OnEntityFinishInit(PlayerEntity entity) {
		//leftHandTr = transform.Find("Camera/CameraFollower/LeftHandSpawnPos");
		//rightHandTr = transform.Find("Camera/CameraFollower/RightHandSpawnPos");

		SwitchHandItem(HotBarCategory.Left,
			GlobalGameResourceEntities.GetAnyResource(inventoryModel.GetSelectedHotBarSlot(HotBarCategory.Left)
				.GetLastItemUUID()));
		
		SwitchHandItem(HotBarCategory.Right,
			GlobalGameResourceEntities.GetAnyResource(inventoryModel.GetSelectedHotBarSlot(HotBarCategory.Right)
				.GetLastItemUUID()));
		
		this.RegisterEvent<OnCurrentHotbarUpdateEvent>(OnCurrentHotbarUpdate)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
	}

	private void OnCurrentHotbarUpdate(OnCurrentHotbarUpdateEvent e) {
		if (e.TopItem!=null && currentHoldItemViewController?.ResourceEntity?.UUID == e.TopItem.UUID) {
			return;
		}
		SwitchHandItem(e.Category, e.TopItem);
	}

	private void LateUpdate() {
		if (playerModel.IsPlayerDead()) {
			return;
		}

	

		if (currentHoldItemViewController != null) {
			if (currentHoldDeployableItemViewController.Item1 != null &&
                currentHoldDeployableItemViewController.Item2) {
				
				deployFailureReason.Value = HoldingDeployableItemStatusCheck(out Quaternion spawnRotation, out Vector3 spawnPosition);
				currentHoldDeployableItemViewController.Item2.transform.rotation = spawnRotation;
				currentHoldDeployableItemViewController.Item2.transform.position = spawnPosition;
			}
			
			if (playerActions.Shoot.WasPressedThisFrame()) {
				currentHoldItemViewController.OnItemStartUse();
			}
			
			if (playerActions.Shoot.IsPressed()) {
				currentHoldItemViewController.OnItemUse();
			}

			if (playerActions.ItemAlt.IsPressed())
			{
				currentHoldItemViewController.OnItemAltUse();
			}

			
			if (playerActions.Shoot.WasReleasedThisFrame()) {
				currentHoldItemViewController.OnItemStopUse();
				if (currentHoldDeployableItemViewController.Item1 != null&&
				    currentHoldDeployableItemViewController.Item2 && deployFailureReason.Value == DeployFailureReason.NoFailure) {
					DeployCurrentHoldDeployableItem();
					
				}
			}
			
			if (playerActions.Scope.WasPressedThisFrame()) {
				currentHoldItemViewController.OnItemScopePressed();
			}

			//armsHolder.transform.localPosition = currentHoldItemViewController.HandLocalPosition;
		}
	}
	
	

	private void DeployCurrentHoldDeployableItem() {
		currentHoldDeployableItemViewController.Item1.OnDeploy();
		//currentHoldItemViewController = null;
		currentHoldDeployableItemViewController = (null, null);
		HotBarSlot currentHotBarSlot = inventoryModel.GetSelectedHotBarSlot(currentHand);
		currentHotBarSlot.RemoveLastItem();
		this.Delay(0.5f, () => {
			inventoryModel.ReplenishHotBarSlot(currentHand, currentHotBarSlot);
		});
		
		//replenish the hotbar slot


	}

	private DeployFailureReason HoldingDeployableItemStatusCheck(out Quaternion spawnRotation, out Vector3 spawnPosition){
		CrosshairGroundWallHitInfo hitInfo =
			Crosshair.Singleton.GetGroundWallHitInfoFromCrosshair(deployDistance, deployGroundLayerMask);
		
		
		currentHoldDeployableItemViewController.Item1.CheckCanDeploy(hitInfo.HitNormal, hitInfo.HitPoint, !hitInfo.IsHit,
			out DeployFailureReason reason, out spawnRotation);

		spawnPosition = hitInfo.HitPoint;
		return reason;
	}

	private void SwitchHandItem(HotBarCategory category, IResourceEntity resourceEntity) {
		//inHandResourceViewControllers.TryAdd(category, null);
		

		deployFailureReason.Value = DeployFailureReason.NA;
		IInHandResourceViewController previousViewController = currentHoldItemViewController;

		
		if (previousViewController as Object != null) {
			
			previousViewController.ResourceEntity.IsHolding.Value = false;
			previousViewController.OnStopHold();
			waitingPlayerAnimStatesBeforeNextItem = previousViewController.PlayerAnimStateToWaitWhenStopHold;
			waitingActiveLayers = previousViewController.AnimLayerInfos.ConvertAll(layerInfo => animator.GetLayerIndex(layerInfo.LayerName));
		}
		
		if(currentHoldDeployableItemViewController.Item1 != null){
            currentHoldDeployableItemViewController.Item1.OnPreviewTerminate();
        }
		
		currentHoldItemViewController = null;
		currentHoldDeployableItemViewController = (null, null);
		//justSwitchItem = true;

		if (waitingPlayerAnimStatesBeforeNextItem == null || waitingPlayerAnimStatesBeforeNextItem.Count <= 0) {
			currentHand = category;
			if (resourceEntity != null) {
				GameObject spawnedItem = ResourceVCFactory.Singleton.SpawnInHandResourceVC(resourceEntity, true);
				if (!spawnedItem) {
					return;
				}
				Transform targetTr = rightHandTr; //category == HotBarCategory.Left ? leftHandTr : rightHandTr;
			
				spawnedItem.transform.SetParent(targetTr);
				//spawnedItem.transform.localPosition = Vector3.zero;
				//spawnedItem.transform.localRotation = Quaternion.identity;
				//spawnedItem.transform.localScale = Vector3.one;
			
				currentHoldItemViewController = spawnedItem.GetComponent<IInHandResourceViewController>();
				spawnedItem.transform.localPosition = currentHoldItemViewController.InHandLocalPosition;
				spawnedItem.transform.localRotation = Quaternion.Euler(currentHoldItemViewController.InHandLocalRotation);
				spawnedItem.transform.localScale = currentHoldItemViewController.InHandLocalScale;

				if (currentHoldItemViewController is IInHandDeployableResourceViewController) {
					GameObject deployedPrefab =
						ResourceVCFactory.Singleton.SpawnDeployableResourceVC(
							currentHoldItemViewController.ResourceEntity, true, true);

					currentHoldDeployableItemViewController =
						(deployedPrefab.GetComponent<IDeployableResourceViewController>(), deployedPrefab);
				}
			
				
				currentHoldItemViewController.ResourceEntity.IsHolding.Value = true;
				currentHoldItemViewController.OnStartHold(gameObject);
				this.SendCommand(PlayerSwitchAnimCommand.Allocate(currentHoldItemViewController.AnimLayerInfos));
			
				//animator stop current state
			
				//rightHandItemViewController = inHandResourceViewControllers[category];
			}
			else {
				this.SendCommand(PlayerSwitchAnimCommand.Allocate(new List<AnimLayerInfo>()
					{new AnimLayerInfo() {LayerWeight = 1, LayerName = "NoItem"}}));
			}
		}
		else { //wait until the player animation state is finished
			waitingEntity = resourceEntity;
			waitingCategory = category;
		}
	}

	private void Update() {
		//wait until the player animation state is finished
		if(waitingPlayerAnimStatesBeforeNextItem != null && waitingPlayerAnimStatesBeforeNextItem.Count > 0) {
			bool isPlaying = false;
			foreach (int layer in waitingActiveLayers) {
				AnimatorStateInfo currentLayerStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
				foreach (string stateName in waitingPlayerAnimStatesBeforeNextItem) {
					if (currentLayerStateInfo.IsName(stateName)) {
						if (currentLayerStateInfo.normalizedTime < 1) {
							isPlaying = true;
							break;
						}
					}
				}
			}

			if (!isPlaying) {
				waitingPlayerAnimStatesBeforeNextItem = null;
				SwitchHandItem(waitingCategory, waitingEntity);
				waitingEntity = null;
			}
		}
	}
}
