using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;

namespace Runtime.DataFramework.ViewControllers.Entities {
    public interface IEntityViewController : IController{
        public string ID { get; }
    
        public IEntity Entity { get; }
    
        public void Init(string id, IEntity entity);
    }

    public interface IEnemyViewController : IEntityViewController {
        public IEnemyEntity EnemyEntity { get; }

        IEntity IEntityViewController.Entity => EnemyEntity;
    }
}