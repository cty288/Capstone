using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Pool;
using MikroFramework.UIKit;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using UnityEngine;

public class InventoryUIViewController : AbstractPanel, IController {
    [SerializeField] private GameObject slotPrefab;
    
    private RectTransform slotLayout;
    private IInventorySystem inventorySystem;
    private List<InventorySlotViewController> slotViewControllers = new List<InventorySlotViewController>();
    public override void OnInit() {
        slotLayout = transform.Find("InventoryLayout").GetComponent<RectTransform>();
        inventorySystem = this.GetSystem<IInventorySystem>();
        inventorySystem.InitOnGameStart();
        this.RegisterEvent<OnInventorySlotAddedEvent>(OnInventorySlotAdded)
            .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        
        OnInventorySlotAdded(new OnInventorySlotAddedEvent(){AddedCount = inventorySystem.GetSlotCount()});
    }

    private void OnInventorySlotAdded(OnInventorySlotAddedEvent e) {
        for (int i = 0; i < e.AddedCount; i++) {
            GameObject slot = Instantiate(slotPrefab, slotLayout);
            slotViewControllers.Add(slot.GetComponent<InventorySlotViewController>());
        }
    }

    public override void OnOpen(UIMsg msg) {
       this.RegisterEvent<OnInventorySlotUpdateEvent>(OnInventorySlotUIUpdate)
           .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);

       for (int i = 0; i < inventorySystem.GetSlotCount(); i++) {
           InventorySlotInfo slotInfo = inventorySystem.GetItemsAt(i);
           SetSlotItem(i, slotInfo.TopItem, slotInfo.Items.Count);
       }
    }

    private void OnInventorySlotUIUpdate(OnInventorySlotUpdateEvent e) {
        SetSlotItem(e.UpdatedSlot.SlotIndex, e.UpdatedSlot.TopItem, e.UpdatedSlot.Items.Count);
    }
    
    private void SetSlotItem(int slotIndex, IResourceEntity item, int count) {
        slotViewControllers[slotIndex].SetItem(item, count);
    }

    public override void OnClosed() {
        this.UnRegisterEvent<OnInventorySlotUpdateEvent>(OnInventorySlotUIUpdate);
        foreach (InventorySlotViewController slotViewController in slotViewControllers) {
            slotViewController.Clear();
        }
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
