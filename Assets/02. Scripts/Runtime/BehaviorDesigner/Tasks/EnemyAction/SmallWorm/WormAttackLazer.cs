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
using Runtime.BehaviorDesigner.Tasks.Movement;
using UnityEngine.AI;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormAttackLazer : EnemyAction<HunterWormEntity>
    {
        public SharedGameObject lazerPrefab;
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
        private float bulletAccuracy;
        private GameObject player;
        private float damagePerTick;
        private float damageInterval;
        public LineRenderer lr;
        private NavMeshAgent a;
        public HunterWormMovement movement;
        float maxRange;
        private GameObject lazerInstance;
        private HunterWorm worm;
        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(lazerPrefab.Value, 10, 20);
            playerTrans = GetPlayer().transform;
            player = GetPlayer();
        }

        public override void OnStart()
        {
            worm = this.gameObject.GetComponent<HunterWorm>();
            movement.attacking = true;
            a = this.gameObject.GetComponent<NavMeshAgent>();
            a.speed = 0;
            base.OnStart();
            ended = false;
            spawnInterval = enemyEntity.GetCustomDataValue<float>("attack", "spawnInterval");
            bulletCount = enemyEntity.GetCustomDataValue<int>("attack", "ammoSize");

            bulletSpeed = enemyEntity.GetCustomDataValue<float>("attack", "bulletSpeed");
            bulletCount = enemyEntity.GetCustomDataValue<int>("attack", "bulletCount");
            bulletAccuracy = enemyEntity.GetCustomDataValue<float>("attack", "bulletAccuracy");
            maxRange = enemyEntity.GetCustomDataValue<float>("attack", "lazerMaxRange");
            
            damagePerTick = enemyEntity.GetCustomDataValue<float>("attack", "damagePerTick");
            damageInterval = enemyEntity.GetCustomDataValue<float>("attack", "damageInterval");
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
                SpawnLazer();
                yield return null;
            }
            ended = true;
        }
        void SpawnLazer()
        {
            
            UnityEngine.GameObject b = pool.Allocate();
            lazerInstance = b;
            
            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
               enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
               gameObject, gameObject.GetComponent<ICanDealDamage>(), 50f);

            Vector3 dir = (player.transform.position - this.gameObject.transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(dir);
            b.transform.position = this.gameObject.transform.position;
            b.transform.rotation = rotation;
            b.GetComponent<WormBulletLazer>().SetData(this.gameObject , dir , player , maxRange , damageInterval);
        }
       

        public override void OnEnd()
        {

            base.OnEnd();
            StopAllCoroutines();
        }
        
    }
}
