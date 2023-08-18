using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Enemies;
using Runtime.DataFramework.Entities.Weapons;

namespace Runtime.DataFramework.ViewControllers
{
    public interface IEntityViewController : IController
    {
        public string ID { get; }

        public IEntity Entity { get; }

        public void Init(string id, IEntity entity);
    }

    public interface IEnemyViewController : IEntityViewController
    {
        public IEnemyEntity EnemyEntity { get; }

        IEntity IEntityViewController.Entity => EnemyEntity;
    }

    public interface IWeaponViewController : IEntityViewController
    {
        public IWeaponEntity WeaponEntity { get; }

        IEntity IEntityViewController.Entity => WeaponEntity;
    }
}