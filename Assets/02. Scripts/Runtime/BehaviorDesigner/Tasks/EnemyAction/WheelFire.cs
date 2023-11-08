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
using Unity;
using a;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;

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
        private float numBullets;
        private float fanAngle;





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
            numBullets = enemyEntity.GetCustomDataValue<float>("attack", "numberOfBullets");
            fanAngle = enemyEntity.GetCustomDataValue<float>("attack", "fanAngle");

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
            charge.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
                gameObject, gameObject.GetComponent<ICanDealDamage>(), -1);
            var posToSpawn = this.gameObject.transform.up * 0.828f + this.gameObject.transform.right * 0.013f + this.gameObject.transform.forward * 0.114f + this.transform.position;
            charge.transform.position = posToSpawn;
            charge.gameObject.GetComponent<WheelBullet>().SetData(bulletSpeed, this.gameObject.transform,playerTrans , enemyEntity.CurrentFaction.Value , enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"), gameObject, gameObject.GetComponent<ICanDealDamage>(), -1 , numBullets , fanAngle);
            yield return null;
        }
        public override void OnEnd()
        {
            
        }
    }


}