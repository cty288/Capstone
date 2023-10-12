using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.Spawning.Models.Properties;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace Runtime.Spawning
{
    public interface IDirectorEntity : IEntity, IHaveCustomProperties, IHaveTags
    {
        public IStartingCredits GetStartingCredits();
        public ICreditsPerSecond GetCreditsPerSecond();
        public ISpawnTimer GetSpawnTimer();
        public IPackSpawnTimer GetPackSpawnTimer();
    }

    public abstract class DirectorEntity<T> : AbstractBasicEntity, IDirectorEntity where T : DirectorEntity<T>, new()
    {
        public override string EntityName { get; set; }
        
        private IStartingCredits _startingCredits;
        private ICreditsPerSecond _creditsPerSecond;
        private ISpawnTimer _spawnTimer;
        private IPackSpawnTimer _packSpawnTimer;

        protected override ConfigTable GetConfigTable() {
            return null;
        }
        
        public override void OnAwake() {
            base.OnAwake();
            _startingCredits = GetProperty<IStartingCredits>();
            _creditsPerSecond = GetProperty<ICreditsPerSecond>();
            _spawnTimer = GetProperty<ISpawnTimer>();
            _packSpawnTimer = GetProperty<IPackSpawnTimer>();  
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
            this.RegisterInitialProperty<ISpawnTimer>(new SpawnTimer());
            this.RegisterInitialProperty<IPackSpawnTimer>(new PackSpawnTimer());
        }

        public IStartingCredits GetStartingCredits()
        {
            return _startingCredits;
        }

        public ICreditsPerSecond GetCreditsPerSecond()
        {
            return _creditsPerSecond;
        }

        public ISpawnTimer GetSpawnTimer()
        {
            return _spawnTimer;
        }

        public IPackSpawnTimer GetPackSpawnTimer()
        { 
            return _packSpawnTimer;
        }
    }
}