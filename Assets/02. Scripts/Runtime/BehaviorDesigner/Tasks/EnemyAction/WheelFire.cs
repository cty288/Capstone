using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;
using Cinemachine;
using Runtime.Enemies.SmallEnemies;
using UnityEngine.AI;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WheelFire : EnemyAction<SpineWheelEntity>
    {
       
        public SharedGameObject chargePrefab;
        private bool ended;

        private Transform playerTrans;
        private SafeGameObjectPool pool;
        private int bulletCount;
        private float spawnInterval;
        private float bulletSpeed;




        public override void OnAwake()
        {
            
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(chargePrefab.Value, 20, 50);
            playerTrans = GetPlayer().transform;
        }
        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            bulletCount = enemyEntity.GetCustomDataValue<int>("attack", "ammoSize");
            spawnInterval = enemyEntity.GetCustomDataValue<float>("attack", "spawnInterval");
            bulletSpeed = enemyEntity.GetCustomDataValue<float>("attack", "bulletSpeed");
            StartCoroutine(Attack());

        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
        IEnumerator Attack()
        {
            GameObject charge = pool.Allocate();
            var posToSpawn = this.gameObject.transform.up * 1 + this.transform.position;
            charge.transform.position = posToSpawn;
<<<<<<< Updated upstream
=======
            
>>>>>>> Stashed changes
            yield return null;
        }
        public override void OnEnd()
        {
            
        }
    }


}