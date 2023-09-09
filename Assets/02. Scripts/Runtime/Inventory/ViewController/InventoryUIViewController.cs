using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using UnityEngine;

public class InventoryUIViewController : AbstractPanel, IController {
    [SerializeField] private GameObject slotPrefab;
    
    private RectTransform slotLayout;
    private IInventorySystem inventorySystem;
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
        }
    }

    public override void OnOpen(UIMsg msg) {
       this.RegisterEvent<OnInventorySlotUpdateEvent>(OnInventorySlotUIUpdate)
           .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
    }

    private void OnInventorySlotUIUpdate(OnInventorySlotUpdateEvent obj) {
        
    }

    public override void OnClosed() {
        this.UnRegisterEvent<OnInventorySlotUpdateEvent>(OnInventorySlotUIUpdate);
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
