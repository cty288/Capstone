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
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using a;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class BerserkerHighNoon : EnemyAction<BerserkerEntity>
    {
        public SharedGameObject simpleShootBulletPrefab;
        public Transform shootPoint;
        private Transform playerTrans;
        private SafeGameObjectPool pool;
        private float bulletSpeed;
        private float spawnInterval;
        private int bulletCount;
        private bool ended;
        public GameObject indicator;
        private SafeGameObjectPool indicatorPool;

        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(simpleShootBulletPrefab.Value, 30, 50);
            indicatorPool = GameObjectPoolManager.Singleton.CreatePool(indicator, 30, 50);
            playerTrans = GetPlayer().transform;
        }
        // Start is called before the first frame update

        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            bulletCount = enemyEntity.GetCustomDataValue<int>("simpleShoot", "bulletCount");
            bulletSpeed = enemyEntity.GetCustomDataValue<float>("simpleShoot", "bulletSpeed");
            spawnInterval = enemyEntity.GetCustomDataValue<float>("simpleShoot", "spawnInterval");
            //bulletPerSpawn = enemyEntity.GetCustomDataValue<int>("damages", "bulletPerSpawn");
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

        IEnumerator SpawnBullet()
        {
            List<GameObject> indicatorList = new List<GameObject>();
            for(int i = 0; i < 10; i++)
            {
                GameObject go = indicatorPool.Allocate();
                go.transform.position = Vector3.zero;
                Vector3 randomPos = Random.insideUnitSphere * 10;
                randomPos += playerTrans.position;
                randomPos.y = playerTrans.position.y;
                go.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                  0,
                  gameObject, gameObject.GetComponent<ICanDealDamage>(), -1);
                NavMeshHit hit;
                // Attempt to find the nearest point on the NavMesh within a specified range
                if (NavMesh.SamplePosition(randomPos, out hit, 30, NavMesh.AllAreas))
                {
                    go.transform.position = hit.position;
                    indicatorList.Add(go);
                }
                else
                {
                    
                }
                // valid ground around the player
            }

            for (int j = 0; j < indicatorList.Count; j++)
            {
                Debug.Log("shooting");
                // Debug.Log(j);
                UnityEngine.GameObject b = pool.Allocate();
                //float angle = j * 60; // Angle between each bullet
                //b.transform.position = this.gameObject.transform.position + new Vector3(0,4,0);
                // b.transform.Rotate(new Vector3(0, angle, 0));
                //b.transform.Translate(new Vector3(0,0,1));
                b.transform.position = shootPoint.position;
                b.transform.position += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                b.transform.rotation = shootPoint.rotation;
                b.transform.eulerAngles += new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 30f), transform.rotation.z);
                //b.transform.rotation = Quaternion.LookRotation(playerTrans.position - (this.transform.position + new Vector3(0, 4, 0)));

                b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                    5,
                    gameObject, gameObject.GetComponent<ICanDealDamage>(), -1);
                b.GetComponent<BerserkerHighBullet>().SetData(bulletSpeed, playerTrans, indicatorList[j]);
                yield return new WaitForSeconds(0.2f);


            }
            ended = true;




        }


    }
}
