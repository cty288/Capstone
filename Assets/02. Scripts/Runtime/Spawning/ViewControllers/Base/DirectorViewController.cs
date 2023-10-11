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
    }
    
    public abstract class DirectorViewController<T>  : AbstractBasicEntityViewController<T>, IDirectorViewController where T : class, IDirectorEntity, new() 
    {
        protected IDirectorModel directorModel;
        protected Lottery m_lottery;
        
        protected ILevelEntity LevelEntity;
        protected int levelNumber;
        
        [SerializeField] public float minSpawnRange;
        [SerializeField] public float maxSpawnRange;
        [ReadOnly(true)] private int currentCredits;
        
        protected override void Awake() {
            base.Awake();
            directorModel = this.GetModel<IDirectorModel>();
            m_lottery = new Lottery();
        }
        
        public virtual void InitDirector(ILevelEntity levelEntity)
        {
            this.LevelEntity = levelEntity;
        }
        
        protected override IEntity OnBuildNewEntity()
        {
            DirectorBuilder<T> builder = directorModel.GetDirectorBuilder<T>();
            return OnInitDirectorEntity(builder);
        }
        
        protected abstract IEntity OnInitDirectorEntity(DirectorBuilder<T> builder);
        
        public void PopulateLottery(List<LevelSpawnCard> spawnCards)
        {
            m_lottery.SetCards(spawnCards);
        }

        public IEnemyEntity GetNextSpawnEntity()
        {
            LevelSpawnCard spawnCard = m_lottery.PickNextCard();
            IEnemyEntity enemyEntity = spawnCard.TemplateEntity;
            
            //get highest rarity of enemy
            int rarity = 0;
            for(int i = 0; i < levelNumber; i++)
            {
                
            }

            return null;
            // return m_lottery.PickNextCard();
        }
    }
}