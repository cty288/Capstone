using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Utilities;
using TMPro;
using UnityEngine;

public class GameTimeDisplayer : AbstractMikroController<MainGame> {
   [SerializeField] private GameObject panel;
   [SerializeField] private GameObject dayDisplayPanel;
   
   
   [SerializeField] private TMP_Text dayCountText;
   [SerializeField] private TMP_Text timeText;
   [SerializeField] private int updateIntervalInMinutes = 5;
   
   [Header("Day Display Panel")]
   [SerializeField] private TMP_Text dayDisplayPanelDayCountText;

   [SerializeField] private TMP_Text levelNameText;
   [SerializeField] private TMP_Text coordinateText;
   [SerializeField] private TMP_Text signalStrengthText;
   [SerializeField] private TMP_Text sandstormProbText;
   [SerializeField] private GameObject probGroup;
   
   
   private DateTime lastUpdateTime;

   private IGameTimeModel gameTimeModel;
   private ILevelModel levelModel;
   private void Awake() {
      gameTimeModel = this.GetModel<IGameTimeModel>();
      levelModel = this.GetModel<ILevelModel>();
      lastUpdateTime = gameTimeModel.GlobalTime.Value;
      UpdateTime();

      levelModel.CurrentLevelCount.RegisterWithInitValue(OnLevelCountChanged)
         .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
      
      gameTimeModel.GlobalTime.RegisterOnValueChanged(OnGlobalTimeChanged)
         .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
      
      /*gameTimeModel.DayCountThisRound.RegisterOnValueChanged(OnDayCountChanged)
         .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);*/
      
      this.RegisterEvent<OnNewDayStart>(OnNewDay).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
      
   }

   private void Start() {
      this.Delay(0.1f, () => {

         OnNewDay(new OnNewDayStart() {
            DayCount = 0
         });
      });
   }

   private void OnNewDay(OnNewDayStart e) {
      if(levelModel.CurrentLevelCount.Value == 0) {
         dayDisplayPanel.SetActive(false);
         return;
      }

      
      this.Delay(0.1f, () => {
         int dayCount = levelModel.CurrentLevel.Value.DayStayed;
         float spawnRandomBossChance = levelModel.RandomBossEncounterEventChance;
         float sandstormProb = levelModel.CurrentLevel.Value.GetSandstormProb();
         if (dayCount <=1) {
            this.Delay(4f, () => {
               ShowDayDisplayPanel(levelModel.CurrentLevel.Value, spawnRandomBossChance, sandstormProb);
            });
         }
         else {
            ShowDayDisplayPanel(levelModel.CurrentLevel.Value, spawnRandomBossChance, sandstormProb);
         }
      });
      
     
     
   }
   
   
   private void ShowDayDisplayPanel(ILevelEntity levelEntity, float spawnRandomBossChance,
      float sandstormProb) {
      probGroup.gameObject.SetActive(levelModel.CurrentLevelCount.Value < LevelModel.MAX_LEVEL);
      
      dayDisplayPanel.SetActive(true);
      dayDisplayPanelDayCountText.text = Localization.GetFormat("TIME_DISPLAY", levelEntity.DayStayed);
      levelNameText.text = Localization.Get(levelEntity.DisplayNameLocalizedKey);
      coordinateText.text =
         $"X.{levelEntity.GetDisplayedCoordinates().Item1} Y.{levelEntity.GetDisplayedCoordinates().Item2}";

      int signalStrength = (Mathf.RoundToInt(spawnRandomBossChance * 100));
      sandstormProb = (Mathf.RoundToInt(sandstormProb * 100));
      signalStrengthText.text = Localization.GetFormat("DAY_INDICATOR_SIGNAL",
         signalStrength >= 100 ? $"<color=red>{signalStrength}%</color>" : $"{signalStrength}%");

      sandstormProbText.text = Localization.GetFormat("DAY_INDICATOR_SANDSTORM",
         sandstormProb >= 100 ? $"<color=red>{sandstormProb}%</color>" : $"{sandstormProb}%");
      
      this.Delay(4f, () => {
         dayDisplayPanel.SetActive(false);
      });
   }

   private void OnGlobalTimeChanged(DateTime arg1, DateTime updatedTime) {
      if ((updatedTime - lastUpdateTime).TotalMinutes >= updateIntervalInMinutes) {
         UpdateTime();
         lastUpdateTime = updatedTime;
      }
   }

   private void OnLevelCountChanged(int level) {
      UpdateTime();
      if (level == 1) {
         //ShowDayDisplayPanel(1);
      }
   }


   private void UpdateTime() {
      if (levelModel.CurrentLevelCount.Value == 0) {
         panel.SetActive(false);
         return;
      }
      if(levelModel.CurrentLevel.Value == null) return;
      panel.SetActive(true);
      dayCountText.text = Localization.GetFormat("TIME_DISPLAY2", levelModel.CurrentLevel.Value.DayStayed);
      DateTime globalTime = gameTimeModel.GlobalTime.Value;
      timeText.text = $"{globalTime.ToString("HH:mm")}";
   }
}
