using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;
using a;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class RapidFire : EnemyAction<Boss1Entity>
    {
        public SharedGameObject bulletPrefab;
       
        
        private bool ended;
        
        public Boss1 boss1vc;
        
        private Transform playerTrans;
        //public SharedTransform playerTrans;

        private SafeGameObjectPool pool;
        private int bulletCount;
        private float bulletSpeed;
        private float spawnInterval;
        private int bulletPerSpawn;
        public override void OnAwake() {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 20, 50);
            playerTrans = GetPlayer().transform;
            
        }

        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            bulletCount = enemyEntity.GetCustomDataValue<int>("damages", "rapidFireBulletCount");
            bulletSpeed = enemyEntity.GetCustomDataValue<float>("damages", "rapidFireBulletSpeed");
            spawnInterval = enemyEntity.GetCustomDataValue<float>("damages", "rapidFireAttackInterval");
            bulletPerSpawn = enemyEntity.GetCustomDataValue<int>("damages", "bulletPerSpawn");
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
           
                StartCoroutine(SpawnBullet());
            yield return null;
        }
        IEnumerator SpawnBullet() {

            for(int j = 0; j < bulletCount; j++)
            {
                // Debug.Log(j);
                UnityEngine.GameObject b = pool.Allocate();
                //float angle = j * 60; // Angle between each bullet
                //b.transform.position = this.gameObject.transform.position + new Vector3(0,4,0);
                // b.transform.Rotate(new Vector3(0, angle, 0));
                //b.transform.Translate(new Vector3(0,0,1));
                b.transform.rotation = this.gameObject.transform.rotation;
                var randomX = Random.Range(-4f, 4f);
                var randomY = Random.Range(0f, 3f);
                b.transform.position = this.gameObject.transform.position + this.gameObject.transform.up * randomY + this.gameObject.transform.right * randomX + new Vector3(0,6,0);
                //b.transform.rotation = Quaternion.LookRotation(playerTrans.position - (this.transform.position + new Vector3(0, 4, 0)));

                b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                    enemyEntity.GetCustomDataValue<int>("damages", "rapidFireDamage"),
                    gameObject, gameObject.GetComponent<ICanDealDamage>(), -1);
                b.GetComponent<Boss1Bullet>().SetData(bulletSpeed , playerTrans);
                yield return new WaitForSeconds(spawnInterval);

            }
            ended = true;




        }
    }
}
