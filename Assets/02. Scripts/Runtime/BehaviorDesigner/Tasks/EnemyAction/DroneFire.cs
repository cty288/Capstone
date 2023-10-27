using System.Collections;
using a;
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
using Runtime.Enemies.SmallEnemies;
using MikroFramework.AudioKit;
using DG.Tweening;
using UnityEngine.AI;
namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class DroneFire : EnemyAction<BeeEntity>
    {
        public SharedGameObject bulletPrefab;
        //public int bulletCount;
        //public float spawnInterval;
        private bool ended;
        //public int bulletSpeed;
        


        private Transform playerTrans;
        //public SharedTransform playerTrans;

        private SafeGameObjectPool pool;
        private int bulletCount;
        private float spawnInterval;
        private float bulletSpeed;
        public float strafeDuration = 2.0f;
        public float strafeDistance = 2.0f;
        private NavMeshAgent agent;
        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 50, 100);
            playerTrans = GetPlayer().transform;
        }

        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            agent = this.gameObject.GetComponent<NavMeshAgent>();
            bulletCount = enemyEntity.GetCustomDataValue<int>("attack", "ammoSize");
            spawnInterval = enemyEntity.GetCustomDataValue<float>("attack", "spawnInterval");
            bulletSpeed = enemyEntity.GetCustomDataValue<float>("attack", "bulletSpeed");
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
                // 30% chance to strafe
                if (Random.value < 0.3f)
                {
                    yield return StartCoroutine(StrafeCoroutine());
                }
                yield return new WaitForSeconds(spawnInterval);
            }
            ended = true;
        }
        IEnumerator StrafeCoroutine()
        {
            Vector3 initialPosition = transform.position;
            Vector3 strafeDirection = Random.Range(0, 2) == 0 ? Vector3.left : Vector3.right;
            Vector3 targetPosition = initialPosition + strafeDirection * strafeDistance;

            agent.SetDestination(targetPosition);

            // Wait for the NavMeshAgent to reach the destination
            while (agent.remainingDistance > 0.1f)
            {
                yield return null;
            }

            // Reverse strafe back to the initial position
            agent.SetDestination(initialPosition);

            // Wait for the NavMeshAgent to reach the initial position
            while (agent.remainingDistance > 0.1f)
            {
                yield return null;
            }
        }
        //make a Strafe coroutine and use tweening and easing so it moves slow -> fast -> slow 
        void SpawnBullet()
        {


            UnityEngine.GameObject b = pool.Allocate();
            var body = this.gameObject.transform.GetChild(0);
            b.transform.position = body.transform.position;
            b.transform.rotation = Quaternion.LookRotation(playerTrans.position - body.transform.position
                                                           );



            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
                gameObject, gameObject.GetComponent<ICanDealDamage>(), -1f);
            b.GetComponent<DroneBullet>().SetData(bulletSpeed);
            
            AudioSystem.Singleton.Play3DSound("Drone_MachineGun", this.gameObject.transform.position , 0.3f);
        }

        public override void OnEnd() {
            base.OnEnd();
            StopAllCoroutines();
        }
    }
}
