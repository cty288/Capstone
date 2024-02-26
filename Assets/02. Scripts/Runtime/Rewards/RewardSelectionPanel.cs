using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MikroFramework.UIKit;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using TMPro;
using UnityEngine;

namespace _02._Scripts.Runtime.Rewards {

	public class RewardSelectionPanel : AbstractPanel, IGameUIPanel {
		[SerializeField]
		private RectTransform slotLayout;

		[SerializeField] 
		private GameObject slotPrefab;
		
		protected List<ResourceSlotViewController> slotViewControllers = new List<ResourceSlotViewController>();
		protected bool awaked = false;
		protected ResourceSlot selectedSlot;
		
		protected virtual void Awake() {
			awaked = true;
		}

		public void ClearSlots() {
			slotViewControllers.ForEach(slot => {
				slot.UnRegisterOnSlotClickedCallback(OnSlotClicked);
				slot.OnInventoryUIClosed();
			
			});
		
			foreach (Transform child in slotLayout) {
				Destroy(child.gameObject);
			}
		
			slotViewControllers.Clear();
			selectedSlot = null;
		}

		public override void OnInit() {
			
		}

		public override void OnOpen(UIMsg msg) {
			
		}

		public override void OnClosed() {
			ClearSlots();
		}

		public async UniTask<IResourceEntity> SelectReward(List<IResourceEntity> rewardsToSelect,
			bool removeUnSelectedFromModel = true) {
			ClearSlots();
			if (rewardsToSelect == null)
				return null;
			if (!awaked) {
				Awake();
			}
			
			slotLayout.DOAnchorPosY(0, 0.3f).SetUpdate(true);;
			int i = 1;
			foreach (IResourceEntity entity in rewardsToSelect) {
				ResourceSlot slot = new ResourceSlot();
				slot.TryAddItem(entity);
				
				if (slot.GetQuantity() == 0) {
					continue;
				}

				RectTransform targetLayout = slotLayout;
				GameObject slotObject = Instantiate(slotPrefab, targetLayout);
				slotObject.transform.SetParent(targetLayout);

				ResourceSlotViewController slotViewController = slotObject.GetComponent<ResourceSlotViewController>();
				slotViewController.SetSlot(slot, false);
				slotViewController.AllowDrag = false;
				slotViewController.SpawnDescription = true;
				slotViewControllers.Add(slotViewController);
				slotViewController.Activate(true, false);
				slotViewController.transform.Find("NumberBG/NumberText").GetComponent<TMP_Text>().text = i.ToString();
				i++;
				slotViewController.RegisterOnSlotClickedCallback(OnSlotClicked);
			}


			await UniTask.WaitUntil(() => selectedSlot != null);
			
			string selectedID = selectedSlot.GetLastItemUUID();
			IResourceEntity selectedEntity = GlobalGameResourceEntities.GetAnyResource(selectedID);

			slotLayout.DOAnchorPosY(-slotLayout.rect.height, 0.3f).SetUpdate(true);
			await UniTask.WaitForSeconds(0.3f, true);

			foreach (IResourceEntity entity in rewardsToSelect) {
				if (entity.UUID != selectedID) {
					if (removeUnSelectedFromModel) {
						GlobalEntities.GetEntityAndModel(entity.UUID).Item2.RemoveEntity(entity.UUID);
					}
				}
			}
			ClearSlots();

			return selectedEntity;
		}

		private void OnSlotClicked(ResourceSlotViewController onClicked) {
			selectedSlot = onClicked.Slot;
			foreach (ResourceSlotViewController viewController in slotViewControllers) {
				viewController.UnRegisterOnSlotClickedCallback(OnSlotClicked);
			}
		}

		public IPanel GetClosePanel() {
			return this;
		}
	}
}