using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Utilities;
using TMPro;
using UnityEngine;

public class GameTimeDisplayer : AbstractMikroController<MainGame> {
   [SerializeField] private GameObject panel;
   [SerializeField] private TMP_Text dayCountText;
   [SerializeField] private TMP_Text timeText;
   [SerializeField] private int updateIntervalInMinutes = 5;
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
   }

   private void OnGlobalTimeChanged(DateTime arg1, DateTime updatedTime) {
      if ((updatedTime - lastUpdateTime).TotalMinutes >= updateIntervalInMinutes) {
         UpdateTime();
         lastUpdateTime = updatedTime;
      }
   }

   private void OnLevelCountChanged(int level) {
      UpdateTime();
   }


   private void UpdateTime() {
      if (levelModel.CurrentLevelCount.Value == 0) {
         panel.SetActive(false);
         return;
      }
      panel.SetActive(true);
      dayCountText.text = Localization.GetFormat("TIME_DISPLAY", gameTimeModel.DayCountThisRound.Value);
      DateTime globalTime = gameTimeModel.GlobalTime.Value;
      timeText.text = $"{globalTime.ToString("HH:mm")}";
   }
}
