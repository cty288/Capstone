using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.Weapons;
using Runtime.Weapons.Model.Base;

namespace Runtime.DataFramework.ViewControllers.Entities {
    public interface IEntityViewController : IController{
        public string ID { get; }
    
        public IEntity Entity { get; }
        
        public void InitWithID(string id);
    
        //public void Init(string id, IEntity entity);
    }

    public interface IEnemyViewController : IEntityViewController {
        public IEnemyEntity EnemyEntity { get; }

        IEntity IEntityViewController.Entity => EnemyEntity;
    }

    public interface IWeaponViewController : IEntityViewController
    {
        public IWeaponEntity WeaponEntity { get; }

        IEntity IEntityViewController.Entity => WeaponEntity;
    }
}