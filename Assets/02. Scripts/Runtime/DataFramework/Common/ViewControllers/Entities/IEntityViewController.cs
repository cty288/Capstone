using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies.Model;
using Runtime.Player;
using Runtime.Utilities.Collision;
using Runtime.Weapons;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace Runtime.DataFramework.ViewControllers.Entities {
    public interface IEntityViewController : IController{
        public string ID { get; }
    
        public IEntity Entity { get; }
        
        public void InitWithID(string id);
        
        public void OnPointByCrosshair();
        
        public void OnUnPointByCrosshair();
        
        public void OnPlayerInteractiveZoneReachable(GameObject player, PlayerInteractiveZone zone);
        
        public void OnPlayerInteractiveZoneNotReachable(GameObject player, PlayerInteractiveZone zone);
        
        /// <summary>
        /// Enter the interactive zone, no matter if reachable or not
        /// </summary>
        /// <param name="player"></param>
        /// <param name="zone"></param>
        
        public void OnPlayerInInteractiveZone(GameObject player, PlayerInteractiveZone zone);
        
        /// <summary>
        /// Exit the interactive zone, no matter if reachable or not
        /// </summary>
        /// <param name="player"></param>
        /// <param name="zone"></param>
        public void OnPlayerExitInteractiveZone(GameObject player, PlayerInteractiveZone zone);
    
        //public void Init(string id, IEntity entity);
    }

    public interface IEnemyViewController : IEntityViewController, IBelongToFaction {
        public IEnemyEntity EnemyEntity { get; }

        IEntity IEntityViewController.Entity => EnemyEntity;
    }

    public interface IWeaponViewController : IEntityViewController
    {
        public IWeaponEntity WeaponEntity { get; }

        IEntity IEntityViewController.Entity => WeaponEntity;
    }
}