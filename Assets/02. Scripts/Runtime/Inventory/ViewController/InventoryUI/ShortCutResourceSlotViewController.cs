using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.ViewController;
using TMPro;
using UnityEngine;

public class ShortCutResourceSlotViewController : ResourceSlotViewController {
   private TMP_Text shortCutText;
   private Transform shortCutTransform;
   private TMP_Text[] currencyTexts;

   protected override void Awake() {
      base.Awake();
      shortCutTransform = transform.Find("ShortCutTag");
      shortCutText = shortCutTransform.Find("ShortCutText").GetComponent<TMP_Text>();
      currencyTexts = transform.Find("CurrencyList").GetComponentsInChildren<TMP_Text>(true);
      shortCutTransform.gameObject.SetActive(false);
   }
   
   public void SetShortCutText(string text) {
      shortCutTransform.gameObject.SetActive(true);
      shortCutText.text = text;
   }

   protected override void OnShow(IResourceEntity topItem) {
      base.OnShow(topItem);
      foreach (TMP_Text text in currencyTexts) {
         text.gameObject.SetActive(false);
      }

      if (topItem is ISkillEntity skill) {
         Dictionary<CurrencyType, int> currencies = skill.GetSkillUseCostOfCurrentLevel();
         int i = 0;
         foreach (KeyValuePair<CurrencyType, int> pair in currencies) {
            if (pair.Value <= 0) {
               continue;
            }
            currencyTexts[i].gameObject.SetActive(true);
            currencyTexts[i].text = $"<sprite index={(int) pair.Key}>{pair.Value}";
            i++;
         }
      }
   }
   
}
