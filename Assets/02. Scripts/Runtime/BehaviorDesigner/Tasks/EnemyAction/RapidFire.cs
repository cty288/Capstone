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

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class RapidFire : EnemyAction<Boss1Entity>
    {
        public SharedGameObject bulletPrefab;
        public int bulletCount;
        public float spawnInterval;
        private bool ended;
        public int bulletSpeed;
        public Boss1 boss1vc;

        public SharedTransform playerTrans;

        private SafeGameObjectPool pool;

        public override void OnAwake() {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 20, 50);
        }

        public override void OnStart()
        {
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
            for(int i = 0; i < bulletCount; i++)
            {
                SpawnBullet();
                yield return new WaitForSeconds(spawnInterval);
            }
            ended = true;
        }
        void SpawnBullet() {


            UnityEngine.GameObject b = pool.Allocate();
            b.transform.position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
            b.transform.rotation = Quaternion.LookRotation(playerTrans.Value.position -
                                        new Vector3(transform.position.x, transform.position.y + 2,
                                            transform.position.z));

            b.GetComponent<Rigidbody>().velocity = b.transform.forward * bulletSpeed;
            
            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                enemyEntity.GetCustomDataValue<int>("damages", "rapidFireDamage"), 
                gameObject);

        }
    }
}
