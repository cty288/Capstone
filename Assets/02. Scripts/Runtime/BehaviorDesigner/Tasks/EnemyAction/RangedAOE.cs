using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.Temporary;
using UnityEngine;
using Runtime.Enemies;
using Runtime.Utilities.Collision;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class RangedAOE : EnemyAction<Boss1Entity>
    {
        
        public SharedGameObject bulletPrefab;
        private int bulletCount;
        private float spawnInterval;
        private bool ended;
        private float bulletTravelTime;
        private Transform playerTrans;

        //some add-on variables that we can use to add juice to ranged projectile actions
        //will have to make a new RangedAction class to contain all these later
        // public Vector3 offset;
        // public string[] collisionTagsToCheck;
        // public float duration, rotationSpeed, beforeTurnSpeed, afterTurnSpeed, defaultDestinationDistance, destroyDelay;
        // //containers
        // Vector3 startPosition, faceDirection, goingToPositionl;
        // float distance;
        // Quaternion rotation;
        // ParticleSystem loopFX, impactFX;

        private SafeGameObjectPool pool;
        //public SharedTransform playerTrans;

        public override void OnAwake() {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 30, 50);
            playerTrans = GetPlayer().transform;
            
        }

        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            bulletCount = enemyEntity.GetCustomDataValue<int>("damages", "rangedAOEBulletCount");
            spawnInterval = enemyEntity.GetCustomDataValue<float>("damages", "rangedAOEAttackSpeed");
            bulletTravelTime = enemyEntity.GetCustomDataValue<float>("damages", "rangedAOEBulletTime");
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
            for (int i = 0; i < 1; i++)
            {
                SpawnBullet();
                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(bulletTravelTime - spawnInterval);
            ended = true;
        }
        void SpawnBullet()
        {
            //to be honest these are just for visuals
            for (int i = 0; i < 15; i++)
            {
                GameObject b = pool.Allocate();
                Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);

                // Generate a random point around the GameObject
                Vector3 randomDirection = Random.onUnitSphere * 50;
                randomDirection.y = 0; // Ensure it's on the same horizontal plane
                Vector3 randomSpawnPoint = spawnPosition + randomDirection;

                // Perform a raycast from the random point to find a target point on the ground
                RaycastHit hit;
                if (Physics.Raycast(randomSpawnPoint + Vector3.up * 50f, Vector3.down, out hit, Mathf.Infinity))
                {
                    Vector3 randomDestination = hit.point;

                    b.transform.position = spawnPosition;
                    b.transform.rotation = Quaternion.LookRotation(randomDestination - randomSpawnPoint);

                    b.GetComponent<Temporary.EnemyBomb>().Init(randomDestination, bulletTravelTime, enemyEntity.CurrentFaction,
                        enemyEntity.GetCustomDataValue<int>("damages", "rangedAOEDamage"), gameObject);
                }
            }

            //biased bullets which will actually kinda try to hit the player
            for (int i = 0; i < 8; i++)
            {
                GameObject b = pool.Allocate();
                Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);

                // Generate a random point around the GameObject
                Vector3 randomDirection = playerTrans.position + Random.onUnitSphere * 4;
                randomDirection.y = 0; // Ensure it's on the same horizontal plane
                Vector3 randomSpawnPoint = spawnPosition + randomDirection;

                // Perform a raycast from the random point to find a target point on the ground
                RaycastHit hit;
                if (Physics.Raycast(randomSpawnPoint + Vector3.up * 50f, Vector3.down, out hit, Mathf.Infinity))
                {
                    Vector3 randomDestination = hit.point;

                    b.transform.position = spawnPosition;
                    b.transform.rotation = Quaternion.LookRotation(randomDestination - randomSpawnPoint);

                    b.GetComponent<Temporary.EnemyBomb>().Init(randomDestination, bulletTravelTime, enemyEntity.CurrentFaction,
                        enemyEntity.GetCustomDataValue<int>("damages", "rangedAOEDamage"), gameObject);
                }
            }
          

        }
    }
}
