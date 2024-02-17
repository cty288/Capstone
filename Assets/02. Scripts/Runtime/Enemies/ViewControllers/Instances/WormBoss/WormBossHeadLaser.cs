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

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossHeadLaser : EnemyAction<WormBossEntity>
    {
        private TaskStatus taskStatus;
        public SharedGameObject firePoint;
        public float laserDuration = 5f;
        
        public SharedGameObject lazerPrefab;
        private SafeGameObjectPool pool;

        private GameObject laserInstance;
        private int laserDamage = 5;
        private float interval = 0.2f;

        public GameObject charging;
        public GameObject charged;
        public GameObject beam;

        private ParticleSystem chargingVFX;
        private ParticleSystem chargedVFX;
        private ParticleSystem beamVFX;

        
        public override void OnStart()
        {
            taskStatus = TaskStatus.Running;
            pool = GameObjectPoolManager.Singleton.CreatePool(lazerPrefab.Value, 1, 3);
            //laserDamage = enemyEntity.GetCustomDataValue<int>("laserBeam", "laserDamage");
            //interval = enemyEntity.GetCustomDataValue<float>("laserBeam", "interval");

            SkillExecute();
        }
        
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        private async UniTask SkillExecute()
        {
            await UniTask.WaitForSeconds(2f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

            await SpawnLazer();
            
            await UniTask.WaitForSeconds(laserDuration,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            beam.SetActive(false);
            
            await UniTask.WaitForSeconds(1f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            taskStatus = TaskStatus.Success;
        }
        
        private async UniTask SpawnLazer()
        {
            if (laserInstance != null)
            {
                pool.Recycle(laserInstance);
                laserInstance = null;
            }
            
            charging.SetActive(true);
            charged.SetActive(true);
            
            await UniTask.WaitForSeconds(4f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            beam.SetActive(true);
            laserInstance = pool.Allocate();
            /*
            laserInstance.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                enemyEntity.GetCustomDataValue<int>("laserBeam", "laserDamage"),
                gameObject, gameObject.GetComponent<ICanDealDamage>(), 50f);
            */
            Vector3 dir = transform.forward.normalized;
            Quaternion rotation = Quaternion.LookRotation(dir);
            laserInstance.transform.parent = firePoint.Value.transform;
            laserInstance.transform.position = firePoint.Value.transform.position;
            laserInstance.transform.rotation = rotation;
            //laserInstance.GetComponent<WormBossLaser>().SetData(interval, laserDamage);
        }
        
        public override void OnEnd()
        {
            if (laserInstance != null)
            {
                pool.Recycle(laserInstance);
                laserInstance = null;
            }
        }
    }
}
