using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Weapons.Model.Base;
using UnityEngine;

public class ModificationSubPanel : InventorySubPanel {
    [SerializeField] private ModificationPanelSlotLayout weaponSlotLayout;
    [SerializeField] private ModificationPanelSlotLayout weaponPartsSlotLayout;
    [SerializeField] private ModificationWeaponPartsPanel weaponPartsPanel;
    [SerializeField] private GameObject noWeaponSelectedHint;
    [SerializeField] private GameObject weaponDisplay;
    [SerializeField] private RenderTexture weaponDisplayTexture;
    [SerializeField] private float weaponRotationSpeed = 50f;
    
    private IInventoryModel inventoryModel;
    private IInventorySystem inventorySystem;
    private IWeaponEntity selectedWeapon;
    private ItemDisplayer currentWeaponDisplayer;
    private ResLoader resLoader;

    private void Awake() {
        inventoryModel = this.GetModel<IInventoryModel>();
        inventorySystem = this.GetSystem<IInventorySystem>();
        resLoader = this.GetUtility<ResLoader>();
    }

    public override void OnSwitchToPanel() {
        base.OnSwitchToPanel();
        weaponSlotLayout.RegisterOnSlotClicked(OnWeaponSlotClicked);

        List<ResourceSlot> weaponSlots = inventoryModel.GetAllSlots((slot => {
            if (slot.IsEmpty()) {
                return false;
            }

            string uuid = slot.GetLastItemUUID();
            IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
            return entity is IWeaponEntity;
        }));
        
        List<ResourceSlot> weaponPartsSlots = inventoryModel.GetAllSlots((slot => {
            if (slot.IsEmpty() && slot is not HotBarSlot) {
                return true;
            }

            string uuid = slot.GetLastItemUUID();
            IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
            return entity is IWeaponPartsEntity;
        }));
        //reorder weaponPartsSlots, so that empty slots are at the end
        weaponPartsSlots = weaponPartsSlots.OrderBy(slot => slot.IsEmpty()).ToList();
        
        weaponSlotLayout.OnOpenPanel(weaponSlots);
        weaponPartsSlotLayout.OnOpenPanel(weaponPartsSlots);
        UpdateSelectedWeapon();
    }

    private void OnWeaponSlotClicked(ResourceSlotViewController vc) { 
        selectedWeapon = GlobalGameResourceEntities.GetAnyResource(vc.Slot.GetLastItemUUID()) as IWeaponEntity;
       UpdateSelectedWeapon();
    }

    private void UpdateSelectedWeapon() {
        if(currentWeaponDisplayer != null) {
            currentWeaponDisplayer.DestroyDisplayer();
            currentWeaponDisplayer = null;
        }
        
        if (selectedWeapon == null) {
            noWeaponSelectedHint.SetActive(true);
            weaponDisplay.SetActive(false);
        }
        else {
            currentWeaponDisplayer = ItemDisplayer.Create(
                resLoader.LoadSync<GameObject>(selectedWeapon.DisplayedModelPrefabName),
                weaponDisplayTexture, weaponRotationSpeed);
            noWeaponSelectedHint.SetActive(false);
            weaponDisplay.SetActive(true);
        }

        weaponPartsPanel.OnShowWeapon(selectedWeapon);
    }

    public override void OnSwitchToOtherPanel() {
        base.OnSwitchToOtherPanel();
        weaponSlotLayout.OnClosePanel();
        weaponPartsSlotLayout.OnClosePanel();
        weaponSlotLayout.UnRegisterOnSlotClicked(OnWeaponSlotClicked);
        selectedWeapon = null;
        if(currentWeaponDisplayer != null) {
            currentWeaponDisplayer.DestroyDisplayer();
            currentWeaponDisplayer = null;
        }
        weaponPartsPanel.OnClear();
    }
}
