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
        [SerializeField] private bool isDawnDusk = false;
        [SerializeField] private Vector2 daySunRotationEuler = new Vector2(0, 180);
        [SerializeField] private Vector2 nightSunRotationEuler = new Vector2(180, 360);

        private float _endOfDayMinutes = (GameTimeModel.NightStartHour - GameTimeModel.NewDayStartHour) * 60; // From start of day (5am) to end of day (8pm)
        private float _inGameHour = GameTimeModel.DayLength / 24f;
        private float _dawnDuskTimer = 0f;
        
        private IGameTimeModel gameTimeModel;
        private ILevelModel levelModel;
        private static readonly int NightDaySlide = Shader.PropertyToID("_NightDaySlide");

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
            isDay = true;
            // transition material to day.
            isDawnDusk = true;
            _dawnDuskTimer = 0;
        }

        private void OnLevelCountChanged(int obj)
        {
            isDay = true;
        }

        private void OnGlobalTimeChanged(DateTime obj)
        {
            if (isDay)
            {
                float dayTimeMinutes = (obj.Hour - GameTimeModel.NewDayStartHour) * 60 + obj.Minute;
                float sunAngle = Mathf.Lerp(daySunRotationEuler.x, daySunRotationEuler.y, dayTimeMinutes / _endOfDayMinutes);
                skyLight.transform.rotation = Quaternion.Euler(sunAngle, 50, 0);
                if (isDawnDusk)
                {
                    _dawnDuskTimer += Time.deltaTime;
                    float t = _dawnDuskTimer / _inGameHour;
                    sandstormGradientMat.SetFloat(NightDaySlide, Mathf.Lerp(0, 1, t));
                    RenderSettings.fogDensity = t * 0.05f;
                    if (t >= 1) isDawnDusk = false;
                }
            }
            else
            {
                float nightTimeMinutes = ((obj.Hour - GameTimeModel.NewDayStartHour) * 60 + obj.Minute) - _endOfDayMinutes;
                float sunAngle = Mathf.Lerp(nightSunRotationEuler.x, nightSunRotationEuler.y, nightTimeMinutes / (24*60 - _endOfDayMinutes));
                skyLight.transform.rotation = Quaternion.Euler(sunAngle, 50, 0);
                if (isDawnDusk)
                {
                    _dawnDuskTimer += Time.deltaTime;
                    float t = _dawnDuskTimer / _inGameHour;
                    sandstormGradientMat.SetFloat(NightDaySlide, Mathf.Lerp(1, 0, t));
                    RenderSettings.fogDensity = t * 0.05f;
                    if (t >= 1) isDawnDusk = false;
                }
            }
            
        }

        private void OnSandStormWarning(OnSandStormWarning e)
        {
        }
        
        private void OnSandStormKillPlayer(OnSandStormKillPlayer e)
        {
        }

        private void OnNightStart(OnNightStart e)
        {
            isDay = false;
        }
        
        private void OnNightApproaching(OnNightApproaching e)
        {
            //start transitioning material to night
            isDawnDusk = true;
            _dawnDuskTimer = 0;
        }
    }
}