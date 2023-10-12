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
        [ReadOnly(true)] private float directorSpawnTimer = 0f;
        
        [ReadOnly(true)] private LevelSpawnCard selectedCard;
        private bool lastSpawnSuccess = false;

        
        protected override void Awake() {
            base.Awake();
            directorModel = this.GetModel<IDirectorModel>();
        }

        protected override void OnEntityStart()
        {
            m_lottery = new Lottery();
            currentCredits = BoundEntity.GetStartingCredits().RealValue;
            creditTimer = addCreditsInterval;
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

            if(directorSpawnTimer <= 0f)
            {
                lastSpawnSuccess = Spawn();
                
                directorSpawnTimer = BoundEntity.GetSpawnTimer().RealValue;
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
            directorSpawnTimer -= Time.deltaTime;
        }
        
        public virtual void SetSpawnCards(List<LevelSpawnCard> spawnCards)
        {
            m_lottery.SetCards(spawnCards);
        }

        protected virtual bool Spawn()
        {
            //check if over max enemies
            
            if (!lastSpawnSuccess)
            {
                //pick card, store the card
                LevelSpawnCard spawnCard = m_lottery.PickNextCard(); //check if next card is null later

                //determine level and cost
                for (int i = spawnCard.MinRarity; i <= spawnCard.MaxRarity; i++)
                {
                    float cost = spawnCard.GetRealSpawnCost(levelNumber, i);
                    if (currentCredits >= cost)
                    {
                        //pick a spot
                        //raycast down from random point within min/max range
                        //determine if can spawn
                        //spawn

                        currentCredits -= cost;
                        break;
                    }
                }
            }
            //on fail spawn, reset timer, discard card
            //on success, spawn(), reset timer, keep card
            return false;
        }

        protected virtual void AddCredits()
        {
            currentCredits += BoundEntity.GetCreditsPerSecond().RealValue;
        }
    }
}