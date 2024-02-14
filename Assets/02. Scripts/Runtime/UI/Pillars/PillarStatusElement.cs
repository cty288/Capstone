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
   [SerializeField] private Vector2 widthRange = new Vector2(100, 800);
   [SerializeField] private TMP_Text progressText;
   [SerializeField] private Transform raritySpawnPoint;
   [SerializeField] private GameObject rarityIndicatorPrefab;
   [SerializeField] private Image sliderFillImage;


   private List<GameObject> spawnedRarityIndicators = new List<GameObject>();
   private float displayedProgress = 0;
   private float targetProgress = 0;
   private CurrencyType currencyType;

   public CurrencyType CurrencyType => currencyType;
   private RectTransform rectTransform;

   private void Awake() {
      rectTransform = GetComponent<RectTransform>();
   }

   private void Update() {
      displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, Time.deltaTime * 5);
      progressText.text = $"<sprite index={(int) currencyType}>{Mathf.RoundToInt(displayedProgress * 100)}%";
   }

   public void SetProgress(int rarity, CurrencyType currencyType, Color color, float progress) {
      if (rarity > spawnedRarityIndicators.Count) {
         SpawnRarityIndicators(rarity);
      }
      
      sliderFillImage.DOColor(color, 0.3f);
      //progressBar.DOValue(progress, 0.3f);
      float targetWidth = Mathf.Lerp(widthRange.x, widthRange.y, progress);
      rectTransform.DOKill();
      rectTransform.DOSizeDelta(new Vector2(targetWidth, rectTransform.sizeDelta.y), 0.3f);
      targetProgress = progress;
      this.currencyType = currencyType;
   }

   private void SpawnRarityIndicators(int rarity) {
      float height = raritySpawnPoint.GetComponent<RectTransform>().rect.height;
      for (int i = 0; i < rarity; i++) {
         GameObject rarityIndicator = Instantiate(rarityIndicatorPrefab, raritySpawnPoint);
         spawnedRarityIndicators.Add(rarityIndicator);
         RectTransform rarityIndicatorRect = rarityIndicator.GetComponent<RectTransform>();
         rarityIndicatorRect.sizeDelta = new Vector2(height, height);
         
      }
   }
}
