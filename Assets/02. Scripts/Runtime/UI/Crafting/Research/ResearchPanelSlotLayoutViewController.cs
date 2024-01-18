using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.RawMaterials.Model.Base;
using UnityEngine;

public class ResearchPanelSlotLayoutViewController : AbstractMikroController<MainGame>
{
	protected RectTransform slotLayout;
	[SerializeField] private GameObject slotPrefab;
	protected List<ResourceSlotViewController> slotViewControllers = new List<ResourceSlotViewController>();
	
	private IInventoryModel inventoryModel;


	private bool awaked = false;
	
	[SerializeField] private bool isRightSide = false;
	
	
	private Action<ResourceSlotViewController> onSlotClicked;

	protected virtual void Awake() {
		slotLayout = transform.Find("ScrollView/Viewport/InventoryLayoutParent/InventoryLayout").GetComponent<RectTransform>();
		inventoryModel = this.GetModel<IInventoryModel>();
		awaked = true;
		
	}

	public void OnUIClosed() {
		slotViewControllers.ForEach(slot => {
			slot.UnRegisterOnSlotClickedCallback(OnSlotClicked);
			slot.OnInventoryUIClosed();
			
		});

		foreach (Transform child in slotLayout) {
			Destroy(child.gameObject);
		}
		
		slotViewControllers.Clear();
	}


	public void OnShowItems(List<ResourceSlot> slots) {
		if(slots == null)
			return;
		
		Awake();
		foreach (ResourceSlot slot in slots) {
			RectTransform targetLayout = slotLayout;
			GameObject slotObject = Instantiate(slotPrefab, targetLayout);
			slotObject.transform.SetParent(targetLayout);

			ResourceSlotViewController slotViewController = slotObject.GetComponent<ResourceSlotViewController>();
			slotViewController.SetSlot(slot, false);
			slotViewController.AllowDrag = false;
			slotViewControllers.Add(slotViewController);
			slotViewController.Activate(true, isRightSide);

			slotViewController.RegisterOnSlotClickedCallback(OnSlotClicked);
		}
	}

	public void AddItem(IResourceEntity entity) {
		ResourceSlotViewController vcWithSameID = null;
		foreach (ResourceSlotViewController slotViewController in slotViewControllers) {
			if (slotViewController.Slot.EntityKey == entity.EntityName) {
				vcWithSameID = slotViewController;
				break;
			}
		}
		
		if (vcWithSameID != null) {
			if (vcWithSameID.Slot.TryAddItem(entity)) {
				return;
			}
		}
		
		foreach (ResourceSlotViewController slotViewController in slotViewControllers) {
			if (slotViewController.Slot.TryAddItem(entity)) {
				return;
			}
		}
		
	}

	private void OnSlotClicked(ResourceSlotViewController vc) {
		onSlotClicked?.Invoke(vc);
	}
	
	public void RegisterOnSlotClicked(Action<ResourceSlotViewController> callback) {
		onSlotClicked += callback;
	}
	
	public void UnRegisterOnSlotClicked(Action<ResourceSlotViewController> callback) {
		onSlotClicked -= callback;
	}
	
	public bool IsAllEmpty() {
		return slotViewControllers.All(slot => slot.Slot.IsEmpty());
	}

	public HashSet<IHaveExpResourceEntity> GetAllExpResources() {
		HashSet<IHaveExpResourceEntity> resources = new HashSet<IHaveExpResourceEntity>();
		foreach (ResourceSlotViewController slotViewController in slotViewControllers) {
			if (!slotViewController.Slot.IsEmpty()) {
				ResourceSlot slot = slotViewController.Slot;
				foreach (string id in slot.GetUUIDList()) {
					IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(id);
					if (entity is IHaveExpResourceEntity expResourceEntity) {
						resources.Add(expResourceEntity);
					}
				}
			}
		}

		return resources;
	}
}
