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
   private ISkillEntity prevSkill;

   public override void Awake() {
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
         if (prevSkill != null) {
            prevSkill.UnregisterOnSkillUpgrade(OnSkillUpgrade);
         }
         skill.RegisterOnSkillUpgrade(OnSkillUpgrade);
         prevSkill = skill;
         OnSkillUpgrade(skill, 0, skill.GetLevel());
      }
   }

   private void OnSkillUpgrade(ISkillEntity skill, int prevLevel, int newLevel) {
      foreach (TMP_Text text in currencyTexts) {
         text.gameObject.SetActive(false);
         Dictionary<CurrencyType, int> currencies = skill.GetSkillUseCostOfCurrentLevel();
         if (currencies == null) continue;
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

   protected override void Clear() {
      base.Clear();
      if (currencyTexts != null) {
         foreach (TMP_Text text in currencyTexts) {
            text.gameObject.SetActive(false);
         }
      }
      if (prevSkill != null) {
         prevSkill.UnregisterOnSkillUpgrade(OnSkillUpgrade);
         prevSkill = null;
      }
      
   }
}
