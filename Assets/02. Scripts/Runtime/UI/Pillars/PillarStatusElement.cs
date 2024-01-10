using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PillarStatusElement : AbstractMikroController<MainGame> {
   [SerializeField] private Slider progressBar;
   [SerializeField] private TMP_Text progressText;
   [SerializeField] private Transform raritySpawnPoint;
   [SerializeField] private GameObject rarityIndicatorPrefab;
   [SerializeField] private Image sliderFillImage;


   private List<GameObject> spawnedRarityIndicators = new List<GameObject>();
   private float displayedProgress = 0;
   private float targetProgress = 0;
   private CurrencyType currencyType;

   private void Update() {
      displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, Time.deltaTime * 5);
      progressText.text = $"<sprite index={(int) currencyType}>{Mathf.RoundToInt(displayedProgress * 100)}%";
   }

   public void SetProgress(int rarity, CurrencyType currencyType, Color color, float progress) {
      if (rarity > spawnedRarityIndicators.Count) {
         SpawnRarityIndicators(rarity);
      }
      
      sliderFillImage.DOColor(color, 0.3f);
      progressBar.DOValue(progress, 0.3f);
      targetProgress = progress;
      this.currencyType = currencyType;
   }

   private void SpawnRarityIndicators(int rarity) {
      for (int i = 0; i < rarity; i++) {
         GameObject rarityIndicator = Instantiate(rarityIndicatorPrefab, raritySpawnPoint);
         spawnedRarityIndicators.Add(rarityIndicator);
      }
   }
}
