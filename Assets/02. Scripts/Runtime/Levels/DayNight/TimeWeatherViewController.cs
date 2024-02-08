using System;
using _02._Scripts.Runtime.Levels.DayNight;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Sandstorm;
using Framework;
using MikroFramework.Architecture;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace _02._Scripts.Runtime.TimeSystem
{
    public class TimeWeatherViewController : AbstractMikroController<MainGame>
    {
        [SerializeField] private Light skyLight;

        [SerializeField] private bool isDay = true;
        [SerializeField] private bool isDawnDusk = false;
        [SerializeField] private Vector2 daySunRotationEuler = new Vector2(0, 180);
        [SerializeField] private Vector2 nightSunRotationEuler = new Vector2(180, 360);

        private float _endOfDayMinutes = (GameTimeModel.NightStartHour - GameTimeModel.NewDayStartHour) * 60; // From start of day (5am) to end of day (8pm)
        private float _inGameHour = GameTimeModel.DayLength / 24f;
        [SerializeField] private float _dawnDuskTimer = 0f;
        private float _duskTime;
        private Volume _volume;
        private SandstormEffect _sandstorm;
        
        private IGameTimeModel gameTimeModel;
        private ILevelModel levelModel;

        private void Awake() {
            gameTimeModel = this.GetModel<IGameTimeModel>();
            levelModel = this.GetModel<ILevelModel>();
            
            this.RegisterEvent<OnSandStormWarning>(OnSandStormWarning);
            this.RegisterEvent<OnSandStormKillPlayer>(OnSandStormKillPlayer);
            this.RegisterEvent<OnNightStart>(OnNightStart);
            this.RegisterEvent<OnNightApproaching>(OnNightApproaching);
            this.RegisterEvent<OnNewDay>(OnNewDay);

            levelModel.CurrentLevelCount.RegisterWithInitValue(OnLevelCountChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
      
            gameTimeModel.GlobalTime.RegisterOnValueChanged(OnGlobalTimeChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
      
            //gameTimeModel.DayCountThisRound.RegisterOnValueChanged(OnDayCountChanged)
              //  .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
              this.RegisterEvent<OnNewDayStart>(OnNewDay).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        }

        private void OnNewDay(OnNewDayStart e)
        {
            isDay = true;
            // transition material to day.
            isDawnDusk = true;
            _dawnDuskTimer = 0;
            _dayTime = 0;
        }
        private void OnNewDay(OnNewDay e)
        {
            isDay = true;
            // transition material to day.
            isDawnDusk = true;
            _dawnDuskTimer = 0;
            _dayTime = 0;
        }

        private void Start()
        {
            UnityEngine.Rendering.VolumeProfile volumeProfile = GetComponent<UnityEngine.Rendering.Volume>()?.profile;
            if(!volumeProfile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));
 
            if(!volumeProfile.TryGet(out _sandstorm)) throw new System.NullReferenceException(nameof(_sandstorm));
        }

        private void OnDayCountChanged()
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
                    if (obj.Hour == GameTimeModel.NewDayStartHour)
                    {
                        float t = obj.Minute / 60f;
                        _sandstorm.nightDaySlide.value = Mathf.Lerp(0, 1, t);
                        RenderSettings.fogDensity = (1 - t) * 0.05f;
                        _dawnDuskTimer = t;
                        if (t >= 1) isDawnDusk = false;
                    }
                    else if (obj.Hour == GameTimeModel.NightStartHour - 1)
                    {
                        float t = obj.Minute / _duskTime;
                        _sandstorm.nightDaySlide.value = Mathf.Lerp(1, 0, t);
                        RenderSettings.fogDensity = t * 0.05f;
                        _dawnDuskTimer = t;
                        if (t >= 1) isDawnDusk = false;
                    }
                    else if(obj.Hour == GameTimeModel.NewDayStartHour + 1)
                    {
                        _sandstorm.nightDaySlide.value = 1;
                        RenderSettings.fogDensity = 0f;
                        isDawnDusk = false;
                    }
                }
            }
            else
            {
                float nightTimeMinutes = ((obj.Hour - GameTimeModel.NewDayStartHour) * 60 + obj.Minute) - _endOfDayMinutes;
                float sunAngle = Mathf.Lerp(nightSunRotationEuler.x, nightSunRotationEuler.y, nightTimeMinutes / (24*60 - _endOfDayMinutes));
                skyLight.transform.rotation = Quaternion.Euler(sunAngle, 50, 0);
            }
            
        }

        private float _timeMinutes = GameTimeModel.DayLength / 1440f;
        private float _dayTime;
        void Update()
        {
            if (isDay)
            {
                _dayTime += Time.deltaTime * _timeMinutes;
                float dayTimeMinutes = _dayTime;
                float sunAngle = Mathf.Lerp(daySunRotationEuler.x, daySunRotationEuler.y,
                    dayTimeMinutes / _endOfDayMinutes);
                skyLight.transform.rotation = Quaternion.Euler(sunAngle, 50, 0);
            }
            else
            {
                _dayTime += Time.deltaTime * _timeMinutes;
                float nightTimeMinutes = _dayTime - _endOfDayMinutes;
                float sunAngle = Mathf.Lerp(nightSunRotationEuler.x, nightSunRotationEuler.y, nightTimeMinutes / (24*60 - _endOfDayMinutes));
                skyLight.transform.rotation = Quaternion.Euler(sunAngle, 50, 0);
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
            _sandstorm.nightDaySlide.value = 0;
            RenderSettings.fogDensity = 0.05f;
            _dayTime = _endOfDayMinutes;
        }
        
        private void OnNightApproaching(OnNightApproaching e)
        {
            //start transitioning material to night
            isDawnDusk = true;
            _dawnDuskTimer = 0;
            _duskTime = e.RemainingMinutes;
        }
    }
}