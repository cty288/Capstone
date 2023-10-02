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
using Runtime.Enemies.SmallEnemies;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class DroneFire : EnemyAction<BeeEntity>
    {
        public SharedGameObject bulletPrefab;
        public int bulletCount;
        public float spawnInterval;
        private bool ended;
        public int bulletSpeed;
        public Bee droneVC;


        private Transform playerTrans;
        //public SharedTransform playerTrans;

        private SafeGameObjectPool pool;

        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 50, 100);
            playerTrans = GetPlayer().transform;
        }

        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            StartCoroutine(RF());
        }
        public override TaskStatus OnUpdate()
        {
            if (ended)
                return TaskStatus.Success;
            else
                return TaskStatus.Running;

        }
        IEnumerator RF()
        {
            for (int i = 0; i < bulletCount; i++)
            {
                SpawnBullet();
                yield return new WaitForSeconds(spawnInterval);
            }
            ended = true;
        }
        void SpawnBullet()
        {


            UnityEngine.GameObject b = pool.Allocate();
            b.transform.position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
            b.transform.rotation = Quaternion.LookRotation(playerTrans.position -
                                                           new Vector3(transform.position.x, transform.position.y + 2,
                                                               transform.position.z));

            
            /*
            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                enemyEntity.GetCustomDataValue<int>("damages", "rapidFireDamage"),
                gameObject);
            */
            
        }
    }
}
