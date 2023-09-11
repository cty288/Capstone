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
       
        [SerializeField] private InventorySlotLayoutViewController mainSlotLayoutViewController;
        [SerializeField] private List<InventorySlotLayoutViewController> hotBarSlotLayoutViewControllersInspector;


        private Dictionary<HotBarCategory, InventorySlotLayoutViewController> hotBarSlotLayoutViewControllers =
            new Dictionary<HotBarCategory, InventorySlotLayoutViewController>();
        
        
        private RubbishSlotViewController rubbishSlotViewController;
        
        //[SerializeField] RectTransform hotbarLayout;
        private IInventoryModel inventoryModel;
        


        //private Dictionary<HotBarCategory, List<ResourceSlotViewController>> hotBarSlotViewControllers =
           // new Dictionary<HotBarCategory, List<ResourceSlotViewController>>();

        public override void OnInit() {
            gameObject.SetActive(true);
            inventoryModel = this.GetModel<IInventoryModel>();
            foreach (InventorySlotLayoutViewController slotLayoutViewController in hotBarSlotLayoutViewControllersInspector) {
                hotBarSlotLayoutViewControllers.Add(slotLayoutViewController.HotBarCategory, slotLayoutViewController);
            }
            
            rubbishSlotViewController = GetComponentInChildren<RubbishSlotViewController>(true);
            
            // inventorySystem.InitOnGameStart();
            this.RegisterEvent<OnInventorySlotAddedEvent>(OnInventorySlotAdded)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            
            this.RegisterEvent<OnInventoryHotBarSlotAddedEvent>(OnInventoryHotBarSlotAdded)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            
           SetInitialSlots();
           gameObject.SetActive(false);
        }

        private void OnInventoryHotBarSlotAdded(OnInventoryHotBarSlotAddedEvent e) {
            if (hotBarSlotLayoutViewControllers.TryGetValue(e.Category, out var controller)) {
                controller.OnInventorySlotAdded(e.AddedSlots, e.AddedCount);
            }
        }

        private void SetInitialSlots() {
            mainSlotLayoutViewController.OnInventorySlotAdded(
                inventoryModel.GetAllSlots(), inventoryModel.GetSlotCount());

            rubbishSlotViewController.SetSlot(new RubbishSlot());
            
            foreach (KeyValuePair<HotBarCategory,InventorySlotLayoutViewController> hotBarSlotLayoutViewController in hotBarSlotLayoutViewControllers) {
                hotBarSlotLayoutViewController.Value.OnInventorySlotAdded(
                    inventoryModel.GetHotBarSlots(hotBarSlotLayoutViewController.Key),
                    inventoryModel.GetHotBarSlotCount(hotBarSlotLayoutViewController.Key));

                if (hotBarSlotLayoutViewController.Value.ShowSlotItemWhenInventoryUIClosed) {
                    hotBarSlotLayoutViewController.Value.OnShowSlotItem();
                }
            }
            
            
        }

        private void OnInventorySlotAdded(OnInventorySlotAddedEvent e) {
            mainSlotLayoutViewController.OnInventorySlotAdded(e.AddedSlots, e.AddedCount);
        }

        public override void OnOpen(UIMsg msg) {
            mainSlotLayoutViewController.OnShowSlotItem();
            
            foreach (KeyValuePair<HotBarCategory,InventorySlotLayoutViewController> hotBarSlotLayoutViewController 
                     in hotBarSlotLayoutViewControllers) {
                
                if (!hotBarSlotLayoutViewController.Value.ShowSlotItemWhenInventoryUIClosed) {
                    hotBarSlotLayoutViewController.Value.OnShowSlotItem();
                }
            }
            
            rubbishSlotViewController.Activate(true);
        }
        
        

        public override void OnClosed() {
            mainSlotLayoutViewController.OnHideSlotItem();
            
            foreach (KeyValuePair<HotBarCategory,InventorySlotLayoutViewController> hotBarSlotLayoutViewController 
                     in hotBarSlotLayoutViewControllers) {
                
                if (!hotBarSlotLayoutViewController.Value.ShowSlotItemWhenInventoryUIClosed) {
                    hotBarSlotLayoutViewController.Value.OnHideSlotItem();
                }
            }
            
            rubbishSlotViewController.Activate(false);
        }

        public IArchitecture GetArchitecture() {
            return MainGame.Interface;
        }


    }
}
