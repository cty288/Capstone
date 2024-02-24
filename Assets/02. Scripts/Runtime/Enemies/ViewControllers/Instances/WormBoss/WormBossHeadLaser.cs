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
        
        public SharedGameObject lazerPrefab;
        private SafeGameObjectPool pool;

        private GameObject laserInstance;
        private int laserDamage;
        private float chargeUpTime;
        private float simpleMissileMaxTurnAngle;
        private float maxRange;
        private float interval;
        private float laserDuration;

        public GameObject charging;
        public GameObject charged;
        public GameObject beam;

        private ParticleSystem chargingVFX;
        private ParticleSystem chargedVFX;
        private ParticleSystem beamVFX;
        
        public override void OnStart()
        {
            base.OnStart();
            
            taskStatus = TaskStatus.Running;
            pool = GameObjectPoolManager.Singleton.CreatePool(lazerPrefab.Value, 1, 3);

            laserDamage = enemyEntity.GetCustomDataValue<int>("laserBeam", "laserDamage");
            chargeUpTime = enemyEntity.GetCustomDataValue<float>("laserBeam", "chargeUpTime");
            simpleMissileMaxTurnAngle = enemyEntity.GetCustomDataValue<float>("laserBeam", "simpleMissileMaxTurnAngle");
            maxRange = enemyEntity.GetCustomDataValue<float>("laserBeam", "maxRange");
            interval = enemyEntity.GetCustomDataValue<float>("laserBeam", "interval");
            laserDuration = enemyEntity.GetCustomDataValue<float>("laserBeam", "laserDuration");

            SkillExecute();
        }
        
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        private async UniTask SkillExecute()
        {
            await UniTask.WaitForSeconds(chargeUpTime,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

            await SpawnLazer();
            
            await UniTask.WaitForSeconds(laserDuration,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            charged.SetActive(false);
            beam.SetActive(false);
            laserInstance.SetActive(false);
            await disableVFX();
            
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
            
            charged.SetActive(true);
            
            Debug.Log($"Charge Start Lazer {Time.time}");

            await UniTask.WaitForSeconds(3f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            Debug.Log($"Fire Lazer {Time.time}");
            charging.SetActive(false);
            beam.SetActive(true);
            laserInstance = pool.Allocate();
            laserInstance.SetActive(true);
            
            Vector3 dir = transform.forward.normalized;
            Quaternion rotation = Quaternion.LookRotation(dir);
            laserInstance.transform.parent = firePoint.Value.transform;
            laserInstance.transform.position = firePoint.Value.transform.position;
            laserInstance.transform.rotation = rotation;
                        
            laserInstance.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                laserDamage, gameObject, gameObject.GetComponent<ICanDealDamage>(), maxRange);

            laserInstance.GetComponent<WormBossLaser>().SetData(interval, laserDamage);
        }
        private async UniTask disableVFX()
        {
            var c = charged.gameObject.GetComponent<VFXTransform>();
            c.initialSize = new Vector3(300, 300, 300);
            c.timer = 0f;
            c.targetSize = Vector3.zero;
            
            var b = beam.gameObject.GetComponent<VFXTransform>();
            b.initialSize = new Vector3(300, 300, 300);
            b.timer = 0f;
            b.targetSize = Vector3.zero;
           
            
            
        }
        
        public override void OnEnd()
        {
            if (laserInstance != null)
            {
                pool.Recycle(laserInstance);
                laserInstance = null;
            }
            charging.SetActive(false);
            var c = charged.gameObject.GetComponent<VFXTransform>();
            c.initialSize = new Vector3(0, 0, 0);
            c.timer = 0f;
            c.targetSize = new Vector3(300, 300, 300);
            charged.SetActive(false);
            var b = beam.gameObject.GetComponent<VFXTransform>();
            b.initialSize = new Vector3(0, 0, 0);
            b.timer = 0f;
            b.targetSize = new Vector3(300, 300, 300);
            beam.SetActive(false);
        }
    }
}
