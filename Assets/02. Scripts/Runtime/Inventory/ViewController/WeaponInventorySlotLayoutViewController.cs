using System.Collections;
using System.Collections.Generic;
using Runtime.Inventory.ViewController;
using UnityEngine;

public class WeaponInventorySlotLayoutViewController : MainInventorySlotLayoutViewController {
    public override void OnSelected(int slotIndex) {
        base.OnSelected(slotIndex);
        for (int i = 0; i < slotViewControllers.Count; i++) {
            slotViewControllers[i].SetSelected(i == slotIndex);
        }
    }
}
