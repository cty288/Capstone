using System;
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
        
        
        
        private IGameTimeModel gameTimeModel;
        private ILevelModel levelModel;
        private void Awake() {
            gameTimeModel = this.GetModel<IGameTimeModel>();
            levelModel = this.GetModel<ILevelModel>();
            
            this.RegisterEvent<OnSandStormWarning>(OnSandStormWarning);
            this.RegisterEvent<OnSandStormKillPlayer>(OnSandStormKillPlayer);

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
            throw new NotImplementedException();
        }

        private void OnSandStormWarning(OnSandStormWarning e)
        {
        }
        
        private void OnSandStormKillPlayer(OnSandStormKillPlayer e)
        {
        }
        
        
    }
}