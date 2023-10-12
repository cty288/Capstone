using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using MikroFramework.Architecture;
using UnityEngine;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Spawning
{
    public interface IDirectorViewController : IEntityViewController {
        IDirectorEntity DirectorEntity => Entity as IDirectorEntity;

        public void PopulateLottery(List<LevelSpawnCard> spawnCards);
        
        public void SetLevelEntity(ILevelEntity levelEntity);
    }
    
    public abstract class DirectorViewController<T>  : AbstractBasicEntityViewController<T>, IDirectorViewController where T : class, IDirectorEntity, new() 
    {
        protected IDirectorModel directorModel;
        protected Lottery m_lottery;
        
        protected ILevelEntity LevelEntity;
        protected int levelNumber;
        
        [SerializeField] public float minSpawnRange;
        [SerializeField] public float maxSpawnRange;
        [SerializeField] public float addCreditsInterval = 1f;
        
        [ReadOnly(true)] private float currentCredits;
        [ReadOnly(true)] private float creditTimer = 0f;
        [ReadOnly(true)] private float spawnTimer = 0f;
        [ReadOnly(true)] private float packSpawnTimer = 0f;
        
        protected override void Awake() {
            base.Awake();
            directorModel = this.GetModel<IDirectorModel>();
        }

        protected override void OnEntityStart()
        {
            m_lottery = new Lottery();
            currentCredits = BoundEntity.GetStartingCredits().RealValue;
            creditTimer = addCreditsInterval;
            spawnTimer = BoundEntity.GetSpawnTimer().RealValue;
            packSpawnTimer = BoundEntity.GetPackSpawnTimer().RealValue;
        }

        protected override IEntity OnBuildNewEntity()
        {
            DirectorBuilder<T> builder = directorModel.GetDirectorBuilder<T>();
            return OnInitDirectorEntity(builder);
        }
        
        protected abstract IEntity OnInitDirectorEntity(DirectorBuilder<T> builder);
        
        public void Update()
        {
            DecrementTimers();
            
            if (creditTimer <= 0f)
            {
                AddCredits();
                creditTimer = addCreditsInterval;
            }

            if(spawnTimer <= 0f)
            {
                Spawn();
                
                spawnTimer = BoundEntity.GetSpawnTimer().RealValue;
            }
        }

        public void SetLevelEntity(ILevelEntity levelEntity)
        {
            LevelEntity = levelEntity;
            levelNumber = levelEntity.GetCurrentLevelCount();
            IEntity ent = OnBuildNewEntity();
            InitWithID(ent.UUID);
        }
        
        private void DecrementTimers()
        {
            creditTimer -= Time.deltaTime;
            spawnTimer -= Time.deltaTime;
            packSpawnTimer -= Time.deltaTime;
        }
        
        public virtual void SetSpawnCards(List<LevelSpawnCard> spawnCards)
        {
            m_lottery.SetCards(spawnCards);
        }

        protected virtual void Spawn()
        {
            //pick card, store the card
            m_lottery.PickNextCard(); //check if next card is null later
            
            //determine level and cost
            //iterate through levels min to max, then decide on highest cost card that can be spawned
            
            
            //pick a spot
            //raycast down from random point within min/max range
            //determine if can spawn
            
            //on fail spawn, reset timer, discard card
            //on success, spawn(), reset timer, keep card
        }

        protected virtual void AddCredits()
        {
            currentCredits += BoundEntity.GetCreditsPerSecond().RealValue;
        }
    }
}