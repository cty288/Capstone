using System.Collections;
using System.Collections.Generic;
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
        
        protected override void Awake() {
            base.Awake();
            directorModel = this.GetModel<IDirectorModel>();
            m_lottery = new Lottery();
        }
        
        public virtual void InitDirector(ILevelEntity levelEntity)
        {
            BoundEntity.SetLevelEntity(levelEntity);
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
    }
}