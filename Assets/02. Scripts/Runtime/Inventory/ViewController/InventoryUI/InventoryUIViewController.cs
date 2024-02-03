using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
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
using UnityEngine.UI;

namespace Runtime.Inventory.ViewController {
    public class InventoryUIViewController : SwitchableRootPanel, IController, IGameUIPanel {
        private InventorySubPanel _inventorySubPanel;
        
        
        public override void OnInit() {
            gameObject.SetActive(true);
           
            _inventorySubPanel = GetComponentInChildren<InventorySubPanel>(true);
            _inventorySubPanel.OnInit();
            this.RegisterEvent<OnOpenSkillUpgradePanel>(OnOpenSkillUpgradePanel)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            base.OnInit();
            gameObject.SetActive(false);
        }
        
        private void OnOpenSkillUpgradePanel(OnOpenSkillUpgradePanel e) {
            UIManager.Singleton.Open<SkillUpgradePanel>(this, e);
        }

        public override void OnOpen(UIMsg msg) {
            _inventorySubPanel.OnOpen(msg);
            base.OnOpen(msg);
        }

        public override void OnClosed() {
            base.OnClosed();
            _inventorySubPanel.OnClosed();
        }
    }
}
