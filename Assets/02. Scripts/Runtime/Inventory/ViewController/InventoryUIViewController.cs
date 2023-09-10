using System;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Inventory.ViewController {
    public class InventoryUIViewController : AbstractPanel, IController {
        [SerializeField] private GameObject slotPrefab;
    
        private RectTransform slotLayout;
        [SerializeField] RectTransform hotbarLayout;
        private IInventoryModel inventoryModel;
        private List<ResourceSlotViewController> slotViewControllers = new List<ResourceSlotViewController>();
        public override void OnInit() {
            slotLayout = transform.Find("InventoryLayout").GetComponent<RectTransform>();
            inventoryModel = this.GetModel<IInventoryModel>();
            // inventorySystem.InitOnGameStart();
            this.RegisterEvent<OnInventorySlotAddedEvent>(OnInventorySlotAdded)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            
           SetInitialSlots();
        }

        private void SetInitialSlots() {
            OnInventorySlotAdded(new OnInventorySlotAddedEvent() {
                AddedCount = inventoryModel.GetSlotCount(),
                StartIndex = 0,
                AddedSlots = inventoryModel.GetAllSlots()
            });
            
            //update hotbar
            int hotBarCount = inventoryModel.GetHotBarSlotCount();
            for (int i = 0; i < hotBarCount; i++) {
                ShowSlotItem(i);
            }
        }

        private void OnInventorySlotAdded(OnInventorySlotAddedEvent e) {
            int hotBarCount = inventoryModel.GetHotBarSlotCount();
            List<ResourceSlot> addedSlots = e.AddedSlots;
            int j = 0;
            for (int i = e.StartIndex; i < e.AddedCount + e.StartIndex; i++) {
                RectTransform targetLayout = i < hotBarCount ? hotbarLayout : slotLayout;
                int indexInHierarchy = i < hotBarCount ? i : i - hotBarCount;
                
                GameObject slot = Instantiate(slotPrefab, targetLayout);
                slot.transform.SetSiblingIndex(indexInHierarchy);
                
                ResourceSlotViewController slotViewController = slot.GetComponent<ResourceSlotViewController>();
                slotViewController.SetSlot(addedSlots[j++]);
                slotViewControllers.Insert(i, slotViewController);
            }
        }

        public override void OnOpen(UIMsg msg) {
            for (int i = 0; i < inventoryModel.GetSlotCount(); i++) {
                bool isHotBar = i < inventoryModel.GetHotBarSlotCount();
                if (!isHotBar) {
                    ShowSlotItem(i);
                }
            }
        }
        
    
        private void ShowSlotItem(int slotIndex) {
            slotViewControllers[slotIndex].Activate(true);
        }

        public override void OnClosed() {
            for (int i = 0; i < inventoryModel.GetSlotCount(); i++) {
                bool isHotBar = i < inventoryModel.GetHotBarSlotCount();
                if (!isHotBar) {
                    ResourceSlotViewController slotViewController = slotViewControllers[i];
                    slotViewController.Activate(false);
                }
            }
        }

        public IArchitecture GetArchitecture() {
            return MainGame.Interface;
        }


    }
}
