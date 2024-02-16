using System;
using _02._Scripts.Runtime.Levels.DayNight;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Sandstorm;
using Framework;
using MikroFramework.Architecture;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace _02._Scripts.Runtime.TimeSystem
{
    public class TimeWeatherViewController : AbstractMikroController<MainGame>
    {
        [SerializeField] private Light skyLight;
        [FormerlySerializedAs("sandstorm")] [SerializeField] private VisualEffect sandstormVFX;

        [SerializeField] private bool isDay = true;
        [SerializeField] private bool isDawnDusk = false;
        [SerializeField] private Vector2 daySunRotationEuler = new Vector2(0, 180);
        [SerializeField] private Vector2 nightSunRotationEuler = new Vector2(180, 360);
        private float _sandstormFogFactor = 0.02f;

        private float _endOfDayMinutes = (GameTimeModel.NightStartHour - GameTimeModel.NewDayStartHour) * 60; // From start of day (5am) to end of day (8pm)
        private float _inGameHour = GameTimeModel.DayLength / 24f;
        private float _dawnDuskTimer = 0f;
        private float _extraFactor = 0;
        private float _nightFog = 0.02f;
        private float _duskTime;
        private Volume _volume;
        private SandstormEffect _sandstorm;
        private float sandstormCountDown = 0;
        
        private IGameTimeModel gameTimeModel;
        private ILevelModel levelModel;

        private static readonly int AlphaID = Shader.PropertyToID("Alpha");
        private static readonly int NSRID = Shader.PropertyToID("NearSimRate");
        private static readonly int FSRID = Shader.PropertyToID("FarSimRate");

        private void Awake() {
            UnityEngine.Rendering.VolumeProfile volumeProfile = GetComponent<UnityEngine.Rendering.Volume>()?.profile;
            if(!volumeProfile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));
 
            if(!volumeProfile.TryGet(out _sandstorm)) throw new System.NullReferenceException(nameof(_sandstorm));
            
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

        void Start()
        {
            
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
            _extraFactor = 0;
            sandstormCountDown = 0;
            RenderSettings.fogDensity = 0;
            sandstormVFX.SetFloat(AlphaID, 0);
            sandstormVFX.SetInt(NSRID, 0);
            sandstormVFX.SetInt(FSRID, 0);
            sandstormVFX.Reinit();
            _sandstorm.sandstormAlpha.value = 0;
        }

        private void OnGlobalTimeChanged(DateTime obj)
        {
            if (isDay)
            {
                if (isDawnDusk)
                {
                    if (obj.Hour == GameTimeModel.NewDayStartHour)
                    {
                        float t = obj.Minute / 60f;
                        _sandstorm.nightDaySlide.value = Mathf.Lerp(0 - (2 * _extraFactor), 1 + (2 * _extraFactor), t);
                        RenderSettings.fogDensity = (1 - t) * _nightFog + _extraFactor;
                        _dawnDuskTimer = t;
                        if (t >= 1) isDawnDusk = false;
                    }
                    else if (obj.Hour == GameTimeModel.NightStartHour - 1)
                    {
                        float t = obj.Minute / _duskTime;
                        _sandstorm.nightDaySlide.value = Mathf.Lerp(1 + (2 * _extraFactor), 0 - (2 * _extraFactor), t);
                        RenderSettings.fogDensity = t * _nightFog + _extraFactor;
                        _dawnDuskTimer = t;
                        if (t >= 1) isDawnDusk = false;
                    }
                    else if(obj.Hour == GameTimeModel.NewDayStartHour + 1)
                    {
                        _sandstorm.nightDaySlide.value = 1 + (2 * _extraFactor);
                        RenderSettings.fogDensity = 0f + _extraFactor;
                        isDawnDusk = false;
                    }
                }
                else
                {
                    _sandstorm.nightDaySlide.value = 1 + (2 * _extraFactor);
                    RenderSettings.fogDensity = 0f + _extraFactor;
                }
            }
            else
            {
                _sandstorm.nightDaySlide.value = 0 - (2 * _extraFactor);
                RenderSettings.fogDensity = _nightFog + _extraFactor;
            }
        }

        private float _firstSandstormTick;
        private float _timeMinutes = 1440f / GameTimeModel.DayLength;
        private float _dayTime;
        void Update()
        {
            if (levelModel.CurrentLevelCount == 0) return;
            
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
            
            if (sandstormCountDown > 0)
            {
                float t = (_dayTime - _firstSandstormTick) / sandstormCountDown;
                _extraFactor = Mathf.Lerp(0, _sandstormFogFactor, t);
                sandstormVFX.SetFloat(AlphaID, Mathf.Lerp(0, 0.7f, t));
                sandstormVFX.SetInt(NSRID, (int)Mathf.Lerp(0, 32, t));
                sandstormVFX.SetInt(FSRID, (int)Mathf.Lerp(0, 32, t));
                _sandstorm.sandstormAlpha.value = Mathf.Lerp(0, 1.12f, t);
                if (t >= 1) sandstormCountDown = 0;
            }
        }

        private void OnSandStormWarning(OnSandStormWarning e)
        {
            //_extraFactor = sandstormFogFactor;
            sandstormCountDown = e.RemainingMinutes;
            _firstSandstormTick = _dayTime;
        }
        
        private void OnSandStormKillPlayer(OnSandStormKillPlayer e)
        {
            
        }

        private void OnNightStart(OnNightStart e)
        {
            isDay = false;
            _sandstorm.nightDaySlide.value = 0 - (2 * _extraFactor);
            RenderSettings.fogDensity = _nightFog + _extraFactor;
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