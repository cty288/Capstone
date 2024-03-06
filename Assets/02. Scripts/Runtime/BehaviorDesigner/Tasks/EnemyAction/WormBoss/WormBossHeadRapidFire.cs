using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using a;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using FIMSpace.FSpine;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies;
using Runtime.Weapons.ViewControllers.Base;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using System.Collections.Generic;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossHeadRapidFire : EnemyAction<WormBossEntity>
    {
        private TaskStatus taskStatus;
        public SharedGameObject firePoint;
        public GameObject trail;
        public SharedGameObject rapidFireBulletPrefab;
        private SafeGameObjectPool pool;
        private SafeGameObjectPool trailPool;
        private GameObject player;
        private List<GameObject> trailList = new List<GameObject>();
        
        private float bulletSpeed;
        private int bulletCountPerWave;
        private float bulletAccuracy;
        private int bulletDamage;
        private int bulletWaves;
        private float bulletWaveInterval;
        private float bulletRange;

        public override void OnStart()
        {
            base.OnStart(); 
            
            taskStatus = TaskStatus.Running;
            pool = GameObjectPoolManager.Singleton.CreatePool(rapidFireBulletPrefab.Value, 30, 50);
            trailPool = GameObjectPoolManager.Singleton.CreatePool(trail, 30, 50);
            
            player = GetPlayer();
            
            bulletSpeed = enemyEntity.GetCustomDataValue<float>("rapidFire", "bulletSpeed");
            bulletCountPerWave = enemyEntity.GetCustomDataValue<int>("rapidFire", "bulletCountPerWave");
            bulletAccuracy = enemyEntity.GetCustomDataValue<float>("rapidFire", "bulletAccuracy");
            bulletDamage = enemyEntity.GetCustomDataValue<int>("rapidFire", "bulletDamage");
            bulletWaves = enemyEntity.GetCustomDataValue<int>("rapidFire", "bulletWaves");
            bulletWaveInterval = enemyEntity.GetCustomDataValue<float>("rapidFire", "bulletWaveInterval");
            bulletRange = enemyEntity.GetCustomDataValue<float>("rapidFire", "bulletRange");

            SkillExecute();
        }
        
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }
        
        private async UniTask SkillExecute()
        {
            await UniTask.WaitForSeconds(3f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

            for (int i = 0; i < 1; i++)
            {
                await RapidFire();
            }
          
            await UniTask.WaitForSeconds(2f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            foreach(GameObject go in trailList)
            {
                trailPool.Recycle(go);
            }
            
            taskStatus = TaskStatus.Success;
        }
        
        void SpawnBullet()
        {
            // Calculate a random rotation offset within a specified range
            Vector3 randomPointAroundPlayer = player.transform.position + Random.insideUnitSphere * 10;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPointAroundPlayer, out hit, 20f, NavMesh.AllAreas))
            {
                randomPointAroundPlayer = hit.position + new Vector3(0,1f,0);
            }
            else
            {
                return;
            }
            
            GameObject b = pool.Allocate();
            GameObject trail = trailPool.Allocate();
            trailList.Add(trail);
            b.transform.position = firePoint.Value.transform.position;
            b.transform.LookAt(randomPointAroundPlayer);
            // b.name = $"bullet {i}";

            var lineRenderer = trail.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, firePoint.Value.transform.position);
            lineRenderer.SetPosition(1, randomPointAroundPlayer);

            // new GameObject($"bullet end {i}").transform.position = randomPointAroundPlayer;
            
            b.GetComponent<IBulletViewController>().Init(
                enemyEntity.CurrentFaction.Value,
                bulletDamage,
                gameObject, 
                gameObject.GetComponent<ICanDealDamage>(), 
                bulletRange
            );

            b.GetComponent<WormBossHeadMine>().SetData(bulletSpeed, randomPointAroundPlayer);
        }
        
        private async UniTask  RapidFire()
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnBullet();
                await UniTask.WaitForSeconds(0.2f,
                    cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            }
        }
    }
}
