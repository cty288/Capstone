using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Spawning
{
    public interface IDirectorEntity : IEntity, IHaveCustomProperties, IHaveTags
    {
    }

    public abstract class DirectorEntity<T> : AbstractBasicEntity, IDirectorEntity where T : DirectorEntity<T>, new()
    {
        public override string EntityName { get; set; }
        
        private int baseStartingCredits;
        private int startingCredits;
        
        private int baseCreditsPerSecond;
        private int creditsPerSecond;

        private float baseSpawnTimer;
        private float spawnTimer;
        
        private float basePackSpawnTimer;
        private float packSpawnTimer;

        protected override ConfigTable GetConfigTable() {
            return null;
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
        }
    }
}