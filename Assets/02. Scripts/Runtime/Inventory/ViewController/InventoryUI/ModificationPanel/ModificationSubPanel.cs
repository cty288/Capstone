using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Weapons.Model.Base;
using TMPro;
using UnityEngine;

public class ModificationSubPanel : SwitchableSubPanel {
    [SerializeField] private ModificationPanelSlotLayout weaponSlotLayout;
    [SerializeField] private ModificationPanelSlotLayout weaponPartsSlotLayout;
    [SerializeField] private ModificationWeaponPartsPanel weaponPartsPanel;
    [SerializeField] private GameObject noWeaponSelectedHint;
    [SerializeField] private GameObject weaponDisplay;
    [SerializeField] private RenderTexture weaponDisplayTexture;
    [SerializeField] private float weaponRotationSpeed = 50f;
    [SerializeField] private GameObject modificationPropertyDescriptionPrefab;
    [SerializeField] private Transform weaponDescriptionPanel;
    [SerializeField] private TMP_Text weaponNameText;
    
    private IInventoryModel inventoryModel;
    private IInventorySystem inventorySystem;
    private IWeaponEntity selectedWeapon;
    private ItemDisplayer currentWeaponDisplayer;
    private ResLoader resLoader;
    private List<ModificationPropertyDescriptionItemViewController> spawnedPropertyDescriptions = new List<ModificationPropertyDescriptionItemViewController>();
    private HashSet<ResourceSlot> weaponPartsSlots = new HashSet<ResourceSlot>();
    private ResourceSlot currentPreviewingSlot = null;
    private Type currentPreviewingWeaponPartBufferType = null;
    private IBuff currentTemporarilyRemovedWeaponPartBuff = null;
    private IBuffSystem buffSystem;
    
    private void Awake() {
        inventoryModel = this.GetModel<IInventoryModel>();
        inventorySystem = this.GetSystem<IInventorySystem>();
        buffSystem = this.GetSystem<IBuffSystem>();
        resLoader = this.GetUtility<ResLoader>();
        this.RegisterEvent<OnWeaponPartsUpdate>(OnWeaponPartsUpdate).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

   

    public override void OnSwitchToPanel() {
        base.OnSwitchToPanel();
        ResourceSlot.currentHoveredSlot.RegisterOnValueChanged(OnCurrentHoveredSlotChanged);
        
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
        this.weaponPartsSlots = new HashSet<ResourceSlot>(weaponPartsSlots);
        
        weaponSlotLayout.OnOpenPanel(weaponSlots);
        weaponPartsSlotLayout.OnOpenPanel(weaponPartsSlots);
        UpdateSelectedWeapon();
    }

    
    /// <summary>
    /// Buff preview
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="currentSlot"></param>
    private void OnCurrentHoveredSlotChanged(ResourceSlotViewController prev, ResourceSlotViewController currentSlot) {
        if(selectedWeapon == null) {
            return;
        }
        
        if (currentSlot != null && currentSlot.Slot.GetQuantity() > 0 && 
            weaponPartsSlots.Contains(currentSlot.Slot) && currentPreviewingSlot == null) { //previewing a new slot
            
            
            IWeaponPartsEntity weaponPartsEntity =
                GlobalGameResourceEntities.GetAnyResource(currentSlot.Slot.GetLastItemUUID()) as IWeaponPartsEntity;
            if (weaponPartsEntity == null) {
                return;
            }
            
            WeaponPartType type = weaponPartsEntity.WeaponPartType;
            HashSet<WeaponPartsSlot> slots = selectedWeapon.GetWeaponPartsSlots(type);
            if(slots == null || slots.Count == 0) {
                return;
            }
            
            IBuff buff = weaponPartsEntity.OnGetBuff(selectedWeapon);
            
            //see if there are empty slots
            bool hasEmptySlots = false;
            foreach (var slot in slots) {
                if (slot.IsEmpty() && slot.CanPlaceItem(weaponPartsEntity)) {
                    hasEmptySlots = true;
                    break;
                }
            }

            if (hasEmptySlots) {
                if (buffSystem.AddBuff(selectedWeapon, weaponPartsEntity, buff)) {
                    currentPreviewingWeaponPartBufferType = buff.GetType();
                }
                else {
                    buff.RecycleToCache();
                    return;
                }
            }
            else {
                //find the first slot that is not empty
                WeaponPartsSlot firstSlot =
                    slots.First(slot => !slot.IsEmpty() && slot.CanPlaceItem(weaponPartsEntity, true));
                if (firstSlot == null) {
                    return;
                }
                
                IWeaponPartsEntity firstSlotWeaponPartsEntity =
                    GlobalGameResourceEntities.GetAnyResource(firstSlot.GetLastItemUUID()) as IWeaponPartsEntity;
                Type firstSlotWeaponPartBuffType = firstSlotWeaponPartsEntity?.BuffType;
                if (firstSlotWeaponPartBuffType == null) {
                    return;
                }

                if (!buffSystem.RemoveBuff(selectedWeapon, firstSlotWeaponPartBuffType, false, out IBuff removedBuff)) {
                    return;
                }
                
                currentTemporarilyRemovedWeaponPartBuff = removedBuff;
                
                if (buffSystem.AddBuff(selectedWeapon, weaponPartsEntity, buff)) {
                    currentPreviewingWeaponPartBufferType = buff.GetType();
                }
                else {
                    buff.RecycleToCache();
                    //add back the removed buff
                    if (buffSystem.AddBuff(selectedWeapon,firstSlotWeaponPartsEntity, removedBuff)) {
                        removedBuff.AutoRecycleWhenEnd = true;
                        currentTemporarilyRemovedWeaponPartBuff = null;
                    }
                    else {
                        removedBuff.RecycleToCache();
                    }
                    return;
                }

            }

            currentPreviewingSlot = currentSlot.Slot;

        } else if(prev != null && prev.Slot.GetQuantity() > 0 &&
                  weaponPartsSlots.Contains(prev.Slot) && currentPreviewingSlot == prev.Slot) { //stop previewing the current slot
            
            IWeaponPartsEntity weaponPartsEntity =
                GlobalGameResourceEntities.GetAnyResource(prev.Slot.GetLastItemUUID()) as IWeaponPartsEntity;
            if (weaponPartsEntity == null || currentPreviewingWeaponPartBufferType == null) {
                return;
            }
            
            //remove the previewing buff
            buffSystem.RemoveBuff(selectedWeapon, currentPreviewingWeaponPartBufferType, true, out IBuff removedBuff);
            currentPreviewingWeaponPartBufferType = null;
            
            if (currentTemporarilyRemovedWeaponPartBuff != null) {
                //add back the removed buff
                IEntity originalDealer = GlobalEntities
                    .GetEntityAndModel(currentTemporarilyRemovedWeaponPartBuff.BuffDealerID).Item1;
                
                if (buffSystem.AddBuff(selectedWeapon, originalDealer, currentTemporarilyRemovedWeaponPartBuff)) {
                    currentTemporarilyRemovedWeaponPartBuff.AutoRecycleWhenEnd = true;
                    currentTemporarilyRemovedWeaponPartBuff = null;
                }
                else {
                    currentTemporarilyRemovedWeaponPartBuff.RecycleToCache();
                    
                    //get a new buff
                    IBuff buff = weaponPartsEntity.OnGetBuff(selectedWeapon);
                    if (buffSystem.AddBuff(selectedWeapon, weaponPartsEntity, buff)) {
                        
                    }
                    else {
                        buff.RecycleToCache();
                    }
                }
            }

            currentPreviewingSlot = null;
        }
        
        RefreshWeaponDetails(selectedWeapon);
    }

    private void OnWeaponSlotClicked(ResourceSlotViewController vc) { 
        selectedWeapon = GlobalGameResourceEntities.GetAnyResource(vc.Slot.GetLastItemUUID()) as IWeaponEntity;
        UpdateSelectedWeapon();
    }
    private void OnWeaponPartsUpdate(OnWeaponPartsUpdate e) {
        if (e.WeaponEntity == selectedWeapon) {
            RefreshWeaponDetails(selectedWeapon);
        }
    }
    private void UpdateSelectedWeapon() {
        if(currentWeaponDisplayer != null) {
            currentWeaponDisplayer.DestroyDisplayer();
            currentWeaponDisplayer = null;
        }
        
        if (selectedWeapon == null) {
            noWeaponSelectedHint.SetActive(true);
            weaponDisplay.SetActive(false);
            //weaponDescriptionPanel.gameObject.SetActive(false);
        }
        else {
            currentWeaponDisplayer = ItemDisplayer.Create(
                resLoader.LoadSync<GameObject>(selectedWeapon.DisplayedModelPrefabName),
                weaponDisplayTexture, weaponRotationSpeed);
            //weaponDescriptionPanel.gameObject.SetActive(true);
            noWeaponSelectedHint.SetActive(false);
            weaponDisplay.SetActive(true);

            
        }
        RefreshWeaponDetails(selectedWeapon);
        weaponPartsPanel.OnShowWeapon(selectedWeapon);
    }

    private void ClearSpawnedDescriptions() {
        foreach (var description in spawnedPropertyDescriptions) {
            Destroy(description.gameObject);
        }

        spawnedPropertyDescriptions.Clear();
    }
    private void RefreshWeaponDetails(IWeaponEntity weaponEntity) {
        ClearSpawnedDescriptions();
        weaponDescriptionPanel.gameObject.SetActive(weaponEntity != null);
        if (weaponEntity == null) {
            return;
        }
        
        
        weaponNameText.text = weaponEntity.GetDisplayName();
        List<ResourcePropertyDescription> propertyDescriptions = weaponEntity.GetResourcePropertyDescriptions();
        
        if (propertyDescriptions is {Count: > 0}) {
            foreach (ResourcePropertyDescription propertyDescription in propertyDescriptions) {
                if (!propertyDescription.display) {
                    continue;
                }
                GameObject propertyDescriptionItem = Instantiate(modificationPropertyDescriptionPrefab, weaponDescriptionPanel);
                propertyDescriptionItem.transform.SetAsLastSibling();

                ModificationPropertyDescriptionItemViewController propertyDescriptionItemViewController =
                propertyDescriptionItem.GetComponent<ModificationPropertyDescriptionItemViewController>();
                    
                propertyDescriptionItemViewController.SetContent(propertyDescription.LocalizedPropertyName,
                        propertyDescription.GetLocalizedDescription(true), propertyDescription.iconName);

                spawnedPropertyDescriptions.Add(propertyDescriptionItemViewController);

            }
        }
       
    }

    public override void OnSwitchToOtherPanel() {
        base.OnSwitchToOtherPanel();
        weaponSlotLayout.OnClosePanel();
        weaponPartsSlotLayout.OnClosePanel();
        weaponSlotLayout.UnRegisterOnSlotClicked(OnWeaponSlotClicked);
        selectedWeapon = null;
        ResourceSlot.currentHoveredSlot.UnRegisterOnValueChanged(OnCurrentHoveredSlotChanged);
        if(currentWeaponDisplayer != null) {
            currentWeaponDisplayer.DestroyDisplayer();
            currentWeaponDisplayer = null;
        }
        weaponPartsPanel.OnClear();
        currentPreviewingSlot = null;
        currentPreviewingWeaponPartBufferType = null;
    }
}
