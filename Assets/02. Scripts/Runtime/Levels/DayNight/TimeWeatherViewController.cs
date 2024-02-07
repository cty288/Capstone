using System;
using _02._Scripts.Runtime.Levels.DayNight;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Sandstorm;
using Framework;
using MikroFramework.Architecture;
using Runtime.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.TimeSystem
{
    public class TimeWeatherViewController : AbstractMikroController<MainGame>
    {
        [SerializeField] private Material sandstormGradientMat;
        [SerializeField] private Light skyLight;

        [SerializeField] private bool isDay = true;
        [SerializeField] private Vector2 daySunRotationEuler = new Vector2(0, 180);
        [SerializeField] private Vector2 nightSunRotationEuler = new Vector2(180, 360);

        private float _endOfDayMinutes = (GameTimeModel.NightStartHour - GameTimeModel.NewDayStartHour) * 60; // From start of day (5am) to end of day (8pm)
        
        private IGameTimeModel gameTimeModel;
        private ILevelModel levelModel;
        private void Awake() {
            gameTimeModel = this.GetModel<IGameTimeModel>();
            levelModel = this.GetModel<ILevelModel>();
            
            this.RegisterEvent<OnSandStormWarning>(OnSandStormWarning);
            this.RegisterEvent<OnSandStormKillPlayer>(OnSandStormKillPlayer);
            this.RegisterEvent<OnNightStart>(OnNightStart);
            this.RegisterEvent<OnNightApproaching>(OnNightApproaching);

            levelModel.CurrentLevelCount.RegisterWithInitValue(OnLevelCountChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
      
            gameTimeModel.GlobalTime.RegisterOnValueChanged(OnGlobalTimeChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
      
            gameTimeModel.DayCountThisRound.RegisterOnValueChanged(OnDayCountChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        }

        private void OnDayCountChanged(int obj)
        {
            throw new NotImplementedException();
        }

        private void OnLevelCountChanged(int obj)
        {
            throw new NotImplementedException();
        }

        private void OnGlobalTimeChanged(DateTime obj)
        { 
            float dayTimeMinutes = (obj.Hour - GameTimeModel.NewDayStartHour) * 60 + obj.Minute;
        }

        private void OnSandStormWarning(OnSandStormWarning e)
        {
        }
        
        private void OnSandStormKillPlayer(OnSandStormKillPlayer e)
        {
        }

        private void OnNightStart(OnNightStart e)
        {
            
        }
        
        private void OnNightApproaching(OnNightApproaching e)
        {
            
        }
    }
}