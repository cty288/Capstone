using UnityEngine;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using DG.Tweening;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Weapons.ViewControllers.Base;
using Runtime.Weapons.ViewControllers.Instances.WormBoss;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossHeadAcid : EnemyAction<WormBossEntity>
    {
        private TaskStatus taskStatus;
        public SharedGameObject[] firePoints;
        
        public SharedGameObject acidChargePrefab;
        public SharedGameObject acidBulletPrefab;
        
        private SafeGameObjectPool acidChargePool;
        private SafeGameObjectPool acidBulletPool;
        private SafeGameObjectPool acidExplosionPool;
        
        List<GameObject> chargeEffects = new List<GameObject>();
        List<GameObject> bulletEffects = new List<GameObject>();
        
        private Transform playerTrans;

        private float bulletSpeed;
        private float chargeUpTime;
        private int bulletExplosionDamage;
        private float timeBetweenShots;
        private float acidExplosionRadius;
        private float bulletRange;
        private int acidTickDamage;
        
        public override void OnStart()
        {
            base.OnStart();
            
            taskStatus = TaskStatus.Running;
            playerTrans = GetPlayer().transform;
            
            acidChargePool = GameObjectPoolManager.Singleton.CreatePool(acidChargePrefab.Value, 3, 9);
            acidBulletPool = GameObjectPoolManager.Singleton.CreatePool(acidBulletPrefab.Value, 3, 9);
            
            bulletSpeed = enemyEntity.GetCustomDataValue<float>("acidAttack", "bulletSpeed");
            chargeUpTime = enemyEntity.GetCustomDataValue<float>("acidAttack", "chargeUpTime");
            bulletExplosionDamage = enemyEntity.GetCustomDataValue<int>("acidAttack", "bulletExplosionDamage");
            timeBetweenShots = enemyEntity.GetCustomDataValue<float>("acidAttack", "timeBetweenShots");
            acidExplosionRadius = enemyEntity.GetCustomDataValue<float>("acidAttack", "acidExplosionRadius");
            bulletRange = enemyEntity.GetCustomDataValue<float>("acidAttack", "bulletRange");
            acidTickDamage = enemyEntity.GetCustomDataValue<int>("acidAttack", "acidTickDamage");
            
            SkillExecute();
        }
        
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        private async UniTask SkillExecute()
        {
            Shuffle(firePoints);

            // start charges
            foreach (var point in firePoints)
            {
                chargeEffects.Add(SpawnChargeEffect(point.Value.transform));
                await UniTask.WaitForSeconds(chargeUpTime, false, PlayerLoopTiming.Update,
                    gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            }
            
            await UniTask.WaitForSeconds(0.5f, false, PlayerLoopTiming.Update,
                gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            // shoot 1,2,3
            for(int i = 0; i < firePoints.Length; i++)
            {
                chargeEffects[i].SetActive(false);
                bulletEffects.Add(SpawnBulletEffect(firePoints[i].Value.transform));
                await UniTask.WaitForSeconds(timeBetweenShots, false, PlayerLoopTiming.Update,
                    gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            }

            await UniTask.WaitForSeconds(1f, false, PlayerLoopTiming.Update,
                gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

            taskStatus = TaskStatus.Success;
        }
        
        private GameObject SpawnChargeEffect(Transform spawnTransform)
        {
            GameObject c = acidChargePool.Allocate();
            c.transform.parent = spawnTransform;
            c.transform.position = spawnTransform.position;
            c.transform.localScale = Vector3.zero;
            c.transform.DOScale(1f, 1.5f).SetEase(Ease.InOutBounce);
            return c;
        }
        
        private GameObject SpawnBulletEffect(Transform spawnTransform)
        {
            GameObject b = acidBulletPool.Allocate();
            b.transform.position = spawnTransform.position;
            
            Vector3 destination = new Vector3(
                playerTrans.position.x + Random.Range(-8, 8),
                0, 
                playerTrans.position.z + Random.Range(-8, 8)
            );
            
            Vector3 dir = destination - spawnTransform.position;
            b.transform.rotation = Quaternion.LookRotation(dir);
            
            b.GetComponent<IBulletViewController>().Init(Faction.Hostile,
                bulletExplosionDamage,
                gameObject, gameObject.GetComponent<ICanDealDamage>(), bulletRange);

            b.GetComponent<WormBossBulletToxic>().SetData(bulletSpeed, acidTickDamage, acidExplosionRadius);
            return b;
        }
        
        public override void OnEnd()
        {
            foreach (var charge in chargeEffects)
                acidChargePool.Recycle(charge);

            foreach (var bullet in bulletEffects)
                acidBulletPool.Recycle(bullet);
            
            chargeEffects.Clear();
            bulletEffects.Clear();
        }
        
        private void Shuffle<T>(T[] arr)
        {
            // Knuth shuffle algorithm :: courtesy of Wikipedia :)
            for (int t = 0; t < arr.Length; t++ )
            {
                T tmp = arr[t];
                int r = Random.Range(t, arr.Length);
                arr[t] = arr[r];
                arr[r] = tmp;
            }
        }
    }
}
