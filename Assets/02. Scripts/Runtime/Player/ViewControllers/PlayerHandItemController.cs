using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Runtime.Controls;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Player;
using Runtime.Utilities;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using Object = UnityEngine.Object;

public class PlayerHandItemController : AbstractMikroController<MainGame> {
	private IInventoryModel inventoryModel;
	[SerializeField] private Transform leftHandTr;
	[SerializeField] private Transform rightHandTr;

	private Dictionary<HotBarCategory, IInHandResourceViewController> inHandResourceViewControllers =
		new Dictionary<HotBarCategory, IInHandResourceViewController>();

	private IGamePlayerModel playerModel;
	private DPunkInputs.PlayerActions playerActions;
	private IInHandResourceViewController rightHandItemViewController = null;
	
	private void Awake() {
		inventoryModel = this.GetModel<IInventoryModel>();
		playerModel = this.GetModel<IGamePlayerModel>();
		
		playerActions = ClientInput.Singleton.GetPlayerActions();
		
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
		SwitchHandItem(e.Category, e.TopItem);
	}

	private void Update() {
		if (playerModel.IsPlayerDead()) {
			return;
		}

		if (rightHandItemViewController != null) {
			if (playerActions.Shoot.WasPressedThisFrame()) {
				rightHandItemViewController.OnItemStartUse();
			}
			
			if (playerActions.Shoot.IsPressed()) {
				rightHandItemViewController.OnItemUse();
			}
			
			if (playerActions.Shoot.WasReleasedThisFrame()) {
				rightHandItemViewController.OnItemStopUse();
			}
			
			if (playerActions.Scope.WasPerformedThisFrame()) {
				rightHandItemViewController.OnItemScopePressed();
			}
		}
	}

	private void SwitchHandItem(HotBarCategory category, IResourceEntity resourceEntity) {
		inHandResourceViewControllers.TryAdd(category, null);

		IInHandResourceViewController previousViewController = inHandResourceViewControllers[category];
		if (previousViewController as Object != null) {
			previousViewController.OnStopHold();
		}
		
		inHandResourceViewControllers[category] = null;
		rightHandItemViewController = null;
		
		if (resourceEntity != null) {
			GameObject spawnedItem = ResourceVCFactory.Singleton.SpawnInHandResourceVC(resourceEntity, true);
			Transform targetTr = category == HotBarCategory.Left ? leftHandTr : rightHandTr;
			
			spawnedItem.transform.SetParent(targetTr);
			spawnedItem.transform.localPosition = Vector3.zero;
			spawnedItem.transform.localRotation = Quaternion.identity;
			spawnedItem.transform.localScale = Vector3.one;
			
			inHandResourceViewControllers[category] = spawnedItem.GetComponent<IInHandResourceViewController>();
			inHandResourceViewControllers[category].OnStartHold(gameObject);
			rightHandItemViewController = inHandResourceViewControllers[category];
		}
		
	}
}
