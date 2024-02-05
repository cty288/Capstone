using System;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
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
    
        public string PrefabID { get; set; }
        public IEntity Entity { get; }
        
        public void InitWithID(string id, bool recycleIfAlreadyExist = true);
        

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

        public IUnRegister RegisterOnEntityViewControllerInit(Action<IEntityViewController> callback);
        
        
        public void UnRegisterOnEntityViewControllerInit(Action<IEntityViewController> callback);

        //public void Init(string id, IEntity entity);
    }

    public interface IEnemyViewController : ICreatureViewController, IBelongToFaction, IHitResponder, ICanDealDamageViewController {
        public IEnemyEntity EnemyEntity { get; }

        IEntity IEntityViewController.Entity => EnemyEntity;
        
  //      public IEnemyEntity OnInitEntity(int level, int rarity);
        
        
    }

    public interface IWeaponViewController : IEntityViewController
    {
        public IWeaponEntity WeaponEntity { get; }

        IEntity IEntityViewController.Entity => WeaponEntity;
    }
}