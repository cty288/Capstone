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
        public float timer;
        private SafeGameObjectPool pool;
        private int bulletCount;
        private float spawnInterval;
        private float bulletSpeed;
        public float strafeDuration = 1.0f;
        public float strafeDistance;
        private NavMeshAgent agent;
        

        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 50, 100);
            playerTrans = GetPlayer().transform;
        }

        public override void OnStart()
        {
            strafeDistance = Random.Range(4f, 7f);
            agent = this.gameObject.GetComponent<NavMeshAgent>();
            timer = Random.Range(1.5f, 3f);
            base.OnStart();
            ended = false;
            bulletCount = enemyEntity.GetCustomDataValue<int>("attack", "ammoSize");
            spawnInterval = enemyEntity.GetCustomDataValue<float>("attack", "spawnInterval");
            bulletSpeed = enemyEntity.GetCustomDataValue<float>("attack", "bulletSpeed");
            StartCoroutine(RF());
        }
        public override TaskStatus OnUpdate()
        {
            timer -= Time.deltaTime;
            if(timer < 0)
            {
                timer = 100f;
                StartCoroutine(StrafeCoroutine());
            }
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
            ended = true;
        }
        IEnumerator StrafeCoroutine()
        {

            Vector3 initialPosition = transform.position;
            Vector3 strafeDirection = Random.Range(0, 2) == 0 ? Vector3.left : Vector3.right;
            Vector3 targetPosition = initialPosition + strafeDirection * strafeDistance;

            float startTime = Time.time;
            float journeyLength = Vector3.Distance(initialPosition, targetPosition);

            // Get the Rigidbody component
            Rigidbody rb = GetComponent<Rigidbody>();

            while (Time.time - startTime < strafeDuration)
            {
                float distanceCovered = (Time.time - startTime) * (journeyLength / strafeDuration);
                float fractionOfJourney = distanceCovered / journeyLength;

                // Apply an easing function (e.g., ease in-out) 
                float easedFraction = Mathf.SmoothStep(0, 1, fractionOfJourney);

                // Calculate the target position for this frame
                Vector3 nextPosition = Vector3.Lerp(initialPosition, targetPosition, easedFraction);

                // Calculate the velocity based on the change in position
                Vector3 velocity = (nextPosition - rb.position) / Time.fixedDeltaTime;

                // Set the velocity of the Rigidbody to move the object
                rb.velocity = velocity;

                // Yield to the physics update
                yield return new WaitForFixedUpdate();
            }

            // Stop the Rigidbody when the strafing duration is over
            rb.velocity = Vector3.zero;

            // Optionally, add a timer here
            timer = Random.Range(1.5f, 3f);
            yield return null;
        }
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
