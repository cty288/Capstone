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
        public int bulletCount;
        public float spawnInterval;
        private bool ended;
        public float bulletTravelTime;
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
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 20, 50);
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
            for (int i = 0; i < bulletCount; i++)
            {
                SpawnBullet();
                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(bulletTravelTime - spawnInterval);
            ended = true;
        }
        void SpawnBullet()
        {
            Debug.Log("start");
            GameObject b = pool.Allocate();
            b.transform.position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
            b.transform.rotation = Quaternion.LookRotation(playerTrans.position -
                new Vector3(transform.position.x, transform.position.y + 2, transform.position.z));

            b.GetComponent<Temporary.EnemyBomb>().Init(playerTrans, bulletTravelTime, enemyEntity.CurrentFaction,
                enemyEntity.GetCustomDataValue<int>("damages", "rangedAOEDamage"), gameObject);

        }
    }
}
