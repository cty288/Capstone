using System.Collections;
using System.Collections.Generic;
using Runtime.Inventory.ViewController;
using TMPro;
using UnityEngine;

public class ShortCutResourceSlotViewController : ResourceSlotViewController {
   private TMP_Text shortCutText;
   private Transform shortCutTransform;

   protected override void Awake() {
      base.Awake();
      shortCutTransform = transform.Find("ShortCutTag");
      shortCutText = shortCutTransform.Find("ShortCutText").GetComponent<TMP_Text>();
      
      shortCutTransform.gameObject.SetActive(false);
   }
   
   public void SetShortCutText(string text) {
      shortCutTransform.gameObject.SetActive(true);
      shortCutText.text = text;
   }
}
