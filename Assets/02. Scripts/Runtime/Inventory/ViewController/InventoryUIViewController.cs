using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Runtime.Inventory.Model;
using UnityEngine;

public class InventoryUIViewController : AbstractPanel, IController {
    [SerializeField] private GameObject slotPrefab;
    
    private RectTransform slotLayout;
    private IInventorySystem inventorySystem;
    public override void OnInit() {
        slotLayout = transform.Find("InventoryLayout").GetComponent<RectTransform>();
        inventorySystem = this.GetSystem<IInventorySystem>();
        
        this.RegisterEvent<OnInventorySlotAddedEvent>(OnInventorySlotAdded)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        
       inventorySystem.InitOnGameStart();
    }

    private void OnInventorySlotAdded(OnInventorySlotAddedEvent e) {
        for (int i = 0; i < e.AddedCount; i++) {
            GameObject slot = Instantiate(slotPrefab, slotLayout);
        }
    }

    public override void OnOpen(UIMsg msg) {
       
    }

    public override void OnClosed() {
        
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
