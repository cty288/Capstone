using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;
using Runtime.Spawning.Models.Properties;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Spawning
{
    public interface IDirectorEntity : IEntity, IHaveCustomProperties, IHaveTags
    {
        public IStartingCredits GetStartingCredits();
        public ICreditsPerSecond GetCreditsPerSecond();
        public IMinSpawnTimer GetMinSpawnTimer();
        public IMaxSpawnTimer GetMaxSpawnTimer();
        public IMaxActiveTime GetMaxActiveTime();
        public IDirectorCooldown GetDirectorCooldown();
        public IMaxDirectorEnemies GetMaxDirectorEnemies();
        
        // public int GetCurrentActiveEnemies();
        // public void IncrementCurrentActiveEnemies();
        // public void DecrementCurrentActiveEnemies();
    }

    public abstract class DirectorEntity<T> : AbstractBasicEntity, IDirectorEntity where T : DirectorEntity<T>, new()
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "Director";
        
        private IStartingCredits _startingCredits;
        private ICreditsPerSecond _creditsPerSecond;
        private IMinSpawnTimer _minSpawnTimer;
        private IMaxSpawnTimer _maxSpawnTimer;
        private IMaxActiveTime _maxActiveTime;
        private IDirectorCooldown _directorCooldown;
        private IMaxDirectorEnemies _maxDirectorEnemies;
        
        // protected int currentActiveEnemies;
        
        protected override ConfigTable GetConfigTable() {
            return null;
        }
        
        public override void OnAwake() {
            base.OnAwake();
            _startingCredits = GetProperty<IStartingCredits>();
            _creditsPerSecond = GetProperty<ICreditsPerSecond>();
            _minSpawnTimer = GetProperty<IMinSpawnTimer>();
            _maxSpawnTimer = GetProperty<IMaxSpawnTimer>();
            _maxActiveTime = GetProperty<IMaxActiveTime>();
            _directorCooldown = GetProperty<IDirectorCooldown>();
            _maxDirectorEnemies = GetProperty<IMaxDirectorEnemies>();
        }

        protected override void OnInitModifiers(int rarity) { //rarity for directors is useless
            int level = GetProperty<ILevelNumberProperty>().BaseValue;
            OnInitLevelModifiers(level);
        }
        
        protected virtual void OnInitLevelModifiers(int level) {
            //init modifiers here
            SetPropertyModifier<float>(new PropertyNameInfo(PropertyName.min_spawn_timer), (base_val) => {
                return base_val - level * 0.1f;
            });
            SetPropertyModifier<float>(new PropertyNameInfo(PropertyName.max_spawn_timer), (base_val) => {
                return base_val - level * 0.1f;
            });
            SetPropertyModifier<float>(new PropertyNameInfo(PropertyName.credits_per_second), (base_val) => {
                return base_val + level * 0.2f;
            });
            SetPropertyModifier<float>(new PropertyNameInfo(PropertyName.starting_credits), (base_val) => {
                return base_val + level * 8f;
            });
        }

        protected override void OnEntityStart(bool isLoadedFromSave) {
        }

        public override void OnDoRecycle() {
            SafeObjectPool<T>.Singleton.Recycle(this as T);
        }

        protected override string OnGetDescription(string defaultLocalizationKey) {
            return null;
        }
        
        protected override void OnEntityRegisterAdditionalProperties() 
        {
            this.RegisterInitialProperty<IStartingCredits>(new StartingCredits());
            this.RegisterInitialProperty<ICreditsPerSecond>(new CreditsPerSecond());
            this.RegisterInitialProperty<IMinSpawnTimer>(new MinSpawnTimer());
            this.RegisterInitialProperty<IMaxSpawnTimer>(new MaxSpawnTimer());
            this.RegisterInitialProperty<ILevelNumberProperty>(new LevelNumber());
            this.RegisterInitialProperty<IMaxActiveTime>(new MaxActiveTime());
            this.RegisterInitialProperty<IDirectorCooldown>(new DirectorCooldown());
            this.RegisterInitialProperty<IMaxDirectorEnemies>(new MaxDirectorEnemies());
        }

        public IStartingCredits GetStartingCredits()
        {
            return _startingCredits;
        }

        public ICreditsPerSecond GetCreditsPerSecond()
        {
            return _creditsPerSecond;
        }

        public IMinSpawnTimer GetMinSpawnTimer()
        {
            return _minSpawnTimer;
        }
        
        public IMaxSpawnTimer GetMaxSpawnTimer()
        {
            return _maxSpawnTimer;
        }
        
        public IMaxActiveTime GetMaxActiveTime()
        {
            return _maxActiveTime;
        }
        public IDirectorCooldown GetDirectorCooldown()
        {
            return _directorCooldown;
        }
        
        public IMaxDirectorEnemies GetMaxDirectorEnemies()
        {
            return _maxDirectorEnemies;
        }

        // public int GetCurrentActiveEnemies()
        // {
        //     return currentActiveEnemies;
        // }

        // public void IncrementCurrentActiveEnemies()
        // {
        //     currentActiveEnemies++;
        // }
        // public void DecrementCurrentActiveEnemies(){
        //     currentActiveEnemies = Mathf.Max(currentActiveEnemies--, 0);
        // }
    }
}