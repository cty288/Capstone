using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Runtime.Inventory.Model;
using Runtime.Weapons.Model.Base;
using UnityEngine;

public class ModificationWeaponPartsPanel : AbstractMikroController<MainGame>
{
    [SerializeField] private GameObject partsSlotLayoutPrefab;


    public void OnClear() {
	    foreach (Transform child in transform) {
		    child.GetComponent<ModificationWeaponPartsSlotLayout>()?.ClearSlots();
		    Destroy(child.gameObject);
	    }
    }
    
    public void OnShowWeapon(IWeaponEntity weaponEntity){
       OnClear();
       if (weaponEntity == null) {
	       return;
       }

       foreach (var enumType in Enum.GetValues(typeof(WeaponPartType))){
	       WeaponPartType type = (WeaponPartType) enumType;
	       HashSet<WeaponPartsSlot> slots = weaponEntity.GetWeaponPartsSlots(type);
	       if(slots != null && slots.Count > 0){
		       GameObject slotLayout = Instantiate(partsSlotLayoutPrefab, transform);
		       slotLayout.GetComponent<ModificationWeaponPartsSlotLayout>().Init(slots, type);
	       }
	       
       }
       
    }
}
