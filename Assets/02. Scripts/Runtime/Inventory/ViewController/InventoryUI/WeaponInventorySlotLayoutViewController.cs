using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Weapons.Model.Base;
using TMPro;
using UnityEngine;

public class WeaponInventorySlotLayoutViewController : MainInventorySlotLayoutViewController {
    //[SerializeField] private TMP_Text ammoText;
    //[SerializeField] private GameObject ammoTextContainer;

    [SerializeField] private GameObject bigSlotPrefab;
    [SerializeField] private GameObject smallSlotPrefab;
    
    private ResourceSlot currentSlot;
    private IWeaponEntity currentWeapon;
    
    
    
    public override void OnSelected(int slotIndex) {
        base.OnSelected(slotIndex);
        /*for (int i = 0; i < slotViewControllers.Count; i++) {
            slotViewControllers[i].SetSelected(i == slotIndex);
        }*/
        //ammoTextContainer.SetActive(false);
        
        if (currentSlot != null) {
            currentSlot.UnregisterOnSlotUpdateCallback(OnCurrentSlotUpdate);
        }


        currentSlot = slotIndex >= 0 ? slotViewControllers[slotIndex].Slot : currentSlot;
        if (currentSlot != null) {
            OnCurrentSlotUpdate(currentSlot, currentSlot.GetLastItemUUID(), currentSlot.GetUUIDList());
            currentSlot.RegisterOnSlotUpdateCallback(OnCurrentSlotUpdate);
        }

    }
    
    public override void OnInventorySlotAdded(List<ResourceSlot> addedSlots, int addedCount) {
        Awake();
       
			
        int j = 0;
			
        for (int i = 0; i < addedCount; i++) {
            RectTransform targetLayout = slotLayout;
            GameObject slot = null;// Instantiate(slotPrefab, targetLayout);
            if (slotViewControllers.Count == 0) {
                slot = Instantiate(bigSlotPrefab, targetLayout);
            }
            else {
                slot = Instantiate(smallSlotPrefab, targetLayout);
                slot.transform.SetAsLastSibling();
            }

            ResourceSlotViewController slotViewController = slot.GetComponent<ResourceSlotViewController>();
           // slotViewController.Awake();
            slotViewController.SetSlot(addedSlots[j++], IsHUDSlotLayout);
            slotViewControllers.Add(slotViewController);
            OnSlotViewControllerSpawned(slotViewController, i);
        }
    }

    private void OnCurrentSlotUpdate(ResourceSlot slot, string itemID, List<string> arg3) {
        /*if (currentWeapon != null) {
            currentWeapon.CurrentAmmo.UnRegisterOnValueChanged(OnAmmoChanged);
        }
        
       // ammoTextContainer.SetActive(false);
        
        if (!String.IsNullOrEmpty(itemID)) {
            IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(itemID);
            if (entity is IWeaponEntity weapon) {
                currentWeapon = weapon;
                //ammoTextContainer.SetActive(true);
                currentWeapon.CurrentAmmo.RegisterWithInitValue(OnAmmoChanged);
            }
        }*/
    }

    public override void OnInventoryUIClosed() {
        
    }

    private void OnDestroy() {
        if (currentSlot != null) {
            currentSlot.UnregisterOnSlotUpdateCallback(OnCurrentSlotUpdate);
        }
        if (currentWeapon != null) {
            currentWeapon.CurrentAmmo.UnRegisterOnValueChanged(OnAmmoChanged);
        }
    }
    
    private void OnAmmoChanged(int num) {
       // ammoText.text = $"Ammo: {num}";
    }

    public override void OnSlotViewControllerSpawned(ResourceSlotViewController slotViewController, int index) {
        base.OnSlotViewControllerSpawned(slotViewController, index);
        (slotViewController as ShortCutResourceSlotViewController)?.SetShortCutText((index + 1).ToString());
    }
    
}
