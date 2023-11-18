using System;
using System.Collections.Generic;
using System.Linq;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Commands;
using Runtime.Inventory.Model;
using Runtime.UI;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Inventory.ViewController {
    public class InventoryUIViewController : AbstractPanelContainer, IController, IGameUIPanel {
       
        [SerializeField] private InventorySlotLayoutViewController mainSlotLayoutViewController;
        [SerializeField] private List<InventorySlotLayoutViewController> hotBarSlotLayoutViewControllersInspector;
        

       

        private Dictionary<HotBarCategory, List<InventorySlotLayoutViewController>> hotBarSlotLayoutViewControllers =
            new Dictionary<HotBarCategory, List<InventorySlotLayoutViewController>>();
        
        
        private RubbishSlotViewController rubbishSlotViewController;
        private UpgradeSlotViewController upgradeSlotViewController;
        
        
        //[SerializeField] RectTransform hotbarLayout;
        private IInventoryModel inventoryModel;
        


        //private Dictionary<HotBarCategory, List<ResourceSlotViewController>> hotBarSlotViewControllers =
           // new Dictionary<HotBarCategory, List<ResourceSlotViewController>>();

        public override void OnInit() {
            gameObject.SetActive(true);
            inventoryModel = this.GetModel<IInventoryModel>();
            foreach (InventorySlotLayoutViewController slotLayoutViewController in hotBarSlotLayoutViewControllersInspector) {
                if (!hotBarSlotLayoutViewControllers.ContainsKey(slotLayoutViewController.HotBarCategory)) {
                    hotBarSlotLayoutViewControllers.Add(slotLayoutViewController.HotBarCategory, new List<InventorySlotLayoutViewController>());
                }

                hotBarSlotLayoutViewControllers[slotLayoutViewController.HotBarCategory].Add(slotLayoutViewController);
            }
            
            rubbishSlotViewController = GetComponentInChildren<RubbishSlotViewController>(true);
            upgradeSlotViewController = GetComponentInChildren<UpgradeSlotViewController>(true);

           
            // inventorySystem.InitOnGameStart();
            this.RegisterEvent<OnInventorySlotAddedEvent>(OnInventorySlotAdded)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            
            this.RegisterEvent<OnInventoryHotBarSlotAddedEvent>(OnInventoryHotBarSlotAdded)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            
            this.RegisterEvent<OnHotBarSlotSelectedEvent>(OnHotBarSlotSelected)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);

            this.RegisterEvent<OnOpenSkillUpgradePanel>(OnOpenSkillUpgradePanel)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            
            
           SetInitialSlots();
           gameObject.SetActive(false);
        }

        private void OnOpenSkillUpgradePanel(OnOpenSkillUpgradePanel e) {
            UIManager.Singleton.Open<SkillUpgradePanel>(this, e);
        }


        private void OnInventoryHotBarSlotAdded(OnInventoryHotBarSlotAddedEvent e) {
            if (hotBarSlotLayoutViewControllers.TryGetValue(e.Category, out var controller)) {
                controller.ForEach(slotLayoutViewController => {
                    slotLayoutViewController.OnInventorySlotAdded(
                        inventoryModel.GetHotBarSlots(e.Category).Select(slot => slot as ResourceSlot).ToList(),
                        inventoryModel.GetHotBarSlotCount(e.Category));
                });
            }
        }

        private void SetInitialSlots() {
            mainSlotLayoutViewController.OnInventorySlotAdded(
                inventoryModel.GetAllSlots(), inventoryModel.GetSlotCount());




            rubbishSlotViewController.SetSlot(new RubbishSlot(), false);
            upgradeSlotViewController.SetSlot(new UpgradeSlot(), false);
            
            foreach (KeyValuePair<HotBarCategory,List<InventorySlotLayoutViewController>> hotBarSlotLayoutViewController in hotBarSlotLayoutViewControllers) {

                List<InventorySlotLayoutViewController> slotLayoutViewControllers =
                    hotBarSlotLayoutViewController.Value;
                    
                foreach (InventorySlotLayoutViewController slotLayoutViewController in slotLayoutViewControllers) {
                    slotLayoutViewController.OnInventorySlotAdded(
                                       inventoryModel.GetHotBarSlots(hotBarSlotLayoutViewController.Key).Select(slot => slot as ResourceSlot).ToList(),
                                       inventoryModel.GetHotBarSlotCount(hotBarSlotLayoutViewController.Key));
                    if (slotLayoutViewController.IsHUDSlotLayout) {
                        slotLayoutViewController.OnShowSlotItem();
                    }
                }
            }

            OnHotBarSlotSelected(new OnHotBarSlotSelectedEvent() {
                Category = HotBarCategory.Left,
                SelectedIndex = inventoryModel.GetSelectedHotBarSlotIndex(HotBarCategory.Left)
            });
            OnHotBarSlotSelected(new OnHotBarSlotSelectedEvent() {
                Category = HotBarCategory.Right,
                SelectedIndex = inventoryModel.GetSelectedHotBarSlotIndex(HotBarCategory.Right)
            });
        }

        private void OnInventorySlotAdded(OnInventorySlotAddedEvent e) {
            mainSlotLayoutViewController.OnInventorySlotAdded(e.AddedSlots, e.AddedCount);
        }

        public override void OnOpen(UIMsg msg) {
            mainSlotLayoutViewController.OnShowSlotItem();
            
            foreach (KeyValuePair<HotBarCategory,List<InventorySlotLayoutViewController>> hotBarSlotLayoutViewController 
                     in hotBarSlotLayoutViewControllers) {
                
                List<InventorySlotLayoutViewController> slotLayoutViewControllers =
                    hotBarSlotLayoutViewController.Value;
                
                foreach (InventorySlotLayoutViewController slotLayoutViewController in slotLayoutViewControllers) {
                    if (!slotLayoutViewController.IsHUDSlotLayout) {
                        slotLayoutViewController.OnShowSlotItem();
                    }
                }
            }
            
            rubbishSlotViewController.Activate(true, true);
            upgradeSlotViewController.Activate(true, true);
        }
        
        
        private void OnHotBarSlotSelected(OnHotBarSlotSelectedEvent e) {
            Debug.Log("OnHotBarSlotSelected");
            HotBarCategory otherCategory =
                e.Category == HotBarCategory.Left ? HotBarCategory.Right : HotBarCategory.Left;
            
            
            if (hotBarSlotLayoutViewControllers.TryGetValue(e.Category, out var controller)) {
                foreach (InventorySlotLayoutViewController slotLayoutViewController in controller) {
                    slotLayoutViewController.OnSelected(e.SelectedIndex);
                }
            }
            
            if (hotBarSlotLayoutViewControllers.TryGetValue(otherCategory, out var otherController)) {
                foreach (InventorySlotLayoutViewController slotLayoutViewController in otherController) {
                    slotLayoutViewController.OnSelected(-1);
                }
            }
        }
        public override void OnClosed() {
            mainSlotLayoutViewController.OnHideSlotItem();
            mainSlotLayoutViewController.OnInventoryUIClosed();
            
            foreach (KeyValuePair<HotBarCategory,List<InventorySlotLayoutViewController>> hotBarSlotLayoutViewController 
                     in hotBarSlotLayoutViewControllers) {

                foreach (InventorySlotLayoutViewController inventorySlotLayoutViewController in hotBarSlotLayoutViewController.Value) {
                    if (!inventorySlotLayoutViewController.IsHUDSlotLayout) {
                        inventorySlotLayoutViewController.OnHideSlotItem();
                    }
                    inventorySlotLayoutViewController.OnInventoryUIClosed();
                }
                
               
            }
            
            rubbishSlotViewController.Activate(false, true);
            upgradeSlotViewController.Activate(false, true);
        }

        public IArchitecture GetArchitecture() {
            return MainGame.Interface;
        }


        public IPanel GetClosePanel() {
            IPanel openedChild = GetTopChild();
            if (openedChild != null) {
                return openedChild;
            }
            
            return this;
        }
    }
}
