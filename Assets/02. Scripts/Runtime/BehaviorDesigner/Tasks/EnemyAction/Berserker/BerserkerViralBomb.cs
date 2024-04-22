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
    public class BerserkerViralBomb : EnemyAction<BerserkerEntity>
    {
        public GameObject viralBomb;
        public Transform shootPoint;
        private SafeGameObjectPool pool;
        private Transform playerTrans;
        private bool ended;
        public GameObject indicator;
        private SafeGameObjectPool indicatorPool;
        private int bulletCount;
        // Start is called before the first frame update

        public override void OnAwake()
        {
            base.OnAwake();
            //pool = GameObjectPoolManager.Singleton.CreatePool(viralBomb,30, 50);
            indicatorPool = GameObjectPoolManager.Singleton.CreatePool(indicator, 30, 50);
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
            StartCoroutine(SpawnBullet());
            yield return null;
        }

        IEnumerator SpawnBullet()
        {
            List<GameObject> indicatorList = new List<GameObject>();
            for (int i = 0; i < 20; i++)
            {
                GameObject go = indicatorPool.Allocate();
                go.transform.position = Vector3.zero;
                Vector3 randomPos = Random.insideUnitSphere * 30;
                randomPos += this.gameObject.transform.position;
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
            ended = true;
            yield return null;
        }
    }
}
