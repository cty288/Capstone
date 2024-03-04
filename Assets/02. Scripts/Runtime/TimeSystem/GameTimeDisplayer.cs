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
   
   [SerializeField] private TMP_Text dayDisplayPanelDayCountText;
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

   private void OnNewDay(OnNewDayStart e) {
      if(levelModel.CurrentLevelCount.Value == 0) {
         dayDisplayPanel.SetActive(false);
         return;
      }

      this.Delay(0.1f, () => {
         int dayCount = levelModel.CurrentLevel.Value.DayStayed;
         if (dayCount <=1) {
            this.Delay(4f, () => {
               ShowDayDisplayPanel(levelModel.CurrentLevel.Value.DayStayed);
            });
         }
         else {
            ShowDayDisplayPanel(levelModel.CurrentLevel.Value.DayStayed);
         }
      });
      
     
     
   }
   
   
   private void ShowDayDisplayPanel(int dayCount) {
      dayDisplayPanel.SetActive(true);
      dayDisplayPanelDayCountText.text = Localization.GetFormat("TIME_DISPLAY", dayCount);
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
      dayCountText.text = Localization.GetFormat("TIME_DISPLAY", levelModel.CurrentLevel.Value.DayStayed);
      DateTime globalTime = gameTimeModel.GlobalTime.Value;
      timeText.text = $"{globalTime.ToString("HH:mm")}";
   }
}
