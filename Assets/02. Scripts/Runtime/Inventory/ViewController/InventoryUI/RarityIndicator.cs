using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class RarityIndicator : MonoBehaviour {
   private Image image;

   private Color normalColor;
   
   [SerializedDictionary("CurrencyType", "Color")]
   [SerializeField]
   private SerializedDictionary<CurrencyType, Color> currencyColors;
   private void Awake() {
      image = GetComponent<Image>();
      normalColor = image.color;
   }

   public void SetCurrency(CurrencyType? currencyType = null) {
      image = GetComponent<Image>();
      
      if (currencyType == null) {
         image.color = normalColor;
         return;
      }

      image.color = currencyColors[currencyType.Value];
   }
   
   public void SetColor(Color color) {
      image.color = color;
   }
}
