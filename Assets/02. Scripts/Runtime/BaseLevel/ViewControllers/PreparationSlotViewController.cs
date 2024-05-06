using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PreparationSlotViewController : MonoBehaviour {
   [SerializeField] private Image[] tickedImages;
   private GameObject newIemHint;
   private Color color = new Color(1, 1, 1, 0);
   private void Awake() {
      foreach (Image tickedImage in tickedImages)
      {
         color.r = tickedImage.color.r;
         color.g = tickedImage.color.g;
         color.b = tickedImage.color.b;
         tickedImage.color = color;
      }
      newIemHint = transform.Find("NewItemHint").gameObject;
   }

   public void SetTicked(bool ticked) {
      if (ticked) {
         foreach (Image tickedImage in tickedImages) {
            tickedImage.DOFade(1, 0.2f).SetUpdate(true);
         }
      }
      else {
         foreach (Image tickedImage in tickedImages) {
            tickedImage.DOFade(0, 0.2f).SetUpdate(true);
         }
      }
   }
   
   public void SetNewItemHint(bool show) {
      newIemHint.SetActive(show);
   }
}
