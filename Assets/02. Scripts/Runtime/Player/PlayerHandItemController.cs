using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using UnityEngine;

public class PlayerHandItemController : AbstractMikroController<MainGame> {
	private IInventoryModel inventoryModel;
	private Transform leftHandTr;
	private Transform rightHandTr;

	private Dictionary<HotBarCategory, IInHandResourceViewController> inHandResourceViewControllers =
		new Dictionary<HotBarCategory, IInHandResourceViewController>();
	private void Awake() {
		inventoryModel = this.GetModel<IInventoryModel>();
		leftHandTr = transform.Find("Cameraroot/LeftHandSpawnPos");
		rightHandTr = transform.Find("Cameraroot/RightHandSpawnPos");

		SwitchHandItem(HotBarCategory.Left,
			GlobalGameResourceEntities.GetAnyResource(inventoryModel.GetSelectedHotBarSlot(HotBarCategory.Left)
				.GetLastItemUUID()));
		
		SwitchHandItem(HotBarCategory.Right,
			GlobalGameResourceEntities.GetAnyResource(inventoryModel.GetSelectedHotBarSlot(HotBarCategory.Right)
				.GetLastItemUUID()));
		
		this.RegisterEvent<OnCurrentHotbarUpdateEvent>(OnCurrentHotbarUpdate)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
	}

	private void OnCurrentHotbarUpdate(OnCurrentHotbarUpdateEvent e) {
		SwitchHandItem(e.Category, e.TopItem);
	}

	private void SwitchHandItem(HotBarCategory category, IResourceEntity resourceEntity) {
		inHandResourceViewControllers.TryAdd(category, null);

		IInHandResourceViewController previousViewController = inHandResourceViewControllers[category];
		if (previousViewController != null) {
			previousViewController.OnStopHold();
		}
		
		inHandResourceViewControllers[category] = null;
		
		if (resourceEntity != null) {
			GameObject spawnedItem = ResourceVCFactory.Singleton.SpawnInHandResourceVC(resourceEntity, true);
			Transform targetTr = category == HotBarCategory.Left ? leftHandTr : rightHandTr;
			
			spawnedItem.transform.SetParent(targetTr);
			spawnedItem.transform.localPosition = Vector3.zero;
			spawnedItem.transform.localRotation = Quaternion.identity;
			spawnedItem.transform.localScale = Vector3.one;

			inHandResourceViewControllers[category] = spawnedItem.GetComponent<IInHandResourceViewController>();
			inHandResourceViewControllers[category].OnStartHold(gameObject);
		}
		
	}
}
