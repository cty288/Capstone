using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using Runtime.Enemies;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossShoot : EnemyAction<WormBossEntity>
    {
        private TaskStatus taskStatus;
        private Transform playerTrans;
        
        private List<GameObject> missileSpawnPos;
        public GameObject missilePrefab;
        private SafeGameObjectPool missilePool;

        private bool startedSpawning;
        public bool shootAllMissiles;
        public bool willEnd = true;
        public SharedBool canShoot;

        private int iterations;
        private float timeBetweenIterations;
        private float timeBetweenMissiles;
        private int missileDamage;
        private float missileTimer;
        private float missileSpeed;
        private float missileMaxTurnAngle;
        private int missileExplosionDamage;
        private float missileExplosionSize;
        
        public override void OnStart()
        {
            base.OnStart();
            
            taskStatus = TaskStatus.Running;
            
            missilePool = GameObjectPoolManager.Singleton.CreatePool(missilePrefab, 20, 50);
            playerTrans = GetPlayer().transform;
            missileSpawnPos = enemyEntity.missileFirePosList;

            iterations = enemyEntity.GetCustomDataValue<int>("missileAttack", "iterations");
            timeBetweenIterations = enemyEntity.GetCustomDataValue<float>("missileAttack", "timeBetweenIterations");
            timeBetweenMissiles = enemyEntity.GetCustomDataValue<float>("missileAttack", "timeBetweenMissiles");
            missileDamage = enemyEntity.GetCustomDataValue<int>("missileAttack", "missileDamage");
            missileTimer = enemyEntity.GetCustomDataValue<float>("missileAttack", "missileLifetime");
            missileSpeed = enemyEntity.GetCustomDataValue<float>("missileAttack", "missileSpeed");
            missileMaxTurnAngle = enemyEntity.GetCustomDataValue<float>("missileAttack", "missileMaxTurnAngle");
            missileExplosionDamage = enemyEntity.GetCustomDataValue<int>("missileAttack", "missileExplosionDamage");
            missileExplosionSize = enemyEntity.GetCustomDataValue<float>("missileAttack", "missileExplosionSize");
        }

        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }
        
        public override void OnLateUpdate()
        {
            base.OnLateUpdate();
            
            if (startedSpawning == false)
            {
                startedSpawning = true;
                SkillExecute();
            }
        }
        
        private async UniTask SkillExecute()
        {
            for (int i = 0; i < iterations; i++)
            {
                for (int j = missileSpawnPos.Count - 1; j >= 0; j--)
                {
                    if (canShoot.Value == false)
                    {
                        return;
                    }
                    
                    GameObject spawnPos = missileSpawnPos[j];

                    if (!shootAllMissiles)
                    {
                        float angle = Vector3.Angle(spawnPos.transform.forward, Vector3.up);
                        if (angle > 100)
                            continue;
                    }
                    else
                    {
                        if (j % 4 != 0) // spawn every 4th point
                            continue;
                    }
                    
                    GameObject b = missilePool.Allocate();
                    
                    b.transform.position = spawnPos.transform.position + spawnPos.transform.forward * 1.2f;
                    
                    b.transform.rotation = Quaternion.Euler(-90, 0, 0);
                    b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value, missileDamage, gameObject, gameObject.GetComponent<ICanDealDamage>(), -1);
                    b.GetComponent<WormBossMissile>().SetData(missileTimer, playerTrans, missileMaxTurnAngle, missileSpeed, missileExplosionDamage, missileExplosionSize);

                    await UniTask.WaitForSeconds(timeBetweenMissiles,
                        cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
                }
                await UniTask.WaitForSeconds(timeBetweenIterations,
                    cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            }

            if (willEnd)
                return;
            
            startedSpawning = false;
        }

        public override void OnEnd()
        {
            startedSpawning = false;
        }
    }
}

