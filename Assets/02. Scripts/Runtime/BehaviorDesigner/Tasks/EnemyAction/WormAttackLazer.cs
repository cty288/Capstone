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
        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(lazerPrefab.Value, 10, 20);
            playerTrans = GetPlayer().transform;
            player = GetPlayer();
           
        }

        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            spawnInterval = enemyEntity.GetCustomDataValue<float>("attack", "spawnInterval");
            bulletCount = enemyEntity.GetCustomDataValue<int>("attack", "ammoSize");

            bulletSpeed = enemyEntity.GetCustomDataValue<float>("attack", "bulletSpeed");
            bulletCount = enemyEntity.GetCustomDataValue<int>("attack", "bulletCount");
            bulletAccuracy = enemyEntity.GetCustomDataValue<float>("attack", "bulletAccuracy");
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
            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
               enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
               gameObject, gameObject.GetComponent<ICanDealDamage>(), 50f);

            b.GetComponent<WormLazer>().SetData(damagePerTick , damageInterval);
            lr = b.GetComponent<LineRenderer>();
            lr.SetPosition(0, this.gameObject.transform.position);
            lr.SetPosition(1, player.gameObject.transform.position);
            b.AddComponent<BoxCollider>();
            b.GetComponent<BoxCollider>().isTrigger = true;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            StopAllCoroutines();
        }
    }
}
