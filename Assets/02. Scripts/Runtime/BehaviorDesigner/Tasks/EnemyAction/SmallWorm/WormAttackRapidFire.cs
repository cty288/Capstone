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
    public class WormAttackRapidFire : EnemyAction<HunterWormEntity>
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
        private float bulletAccuracy;
        private GameObject player;
        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 50, 100);
            playerTrans = GetPlayer().transform;
            player = GetPlayer();
            //Faction faction = enemyEntity.CurrentFaction;
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
            StartCoroutine(RF());
            //StopAllCoroutines();
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
                yield return null;
            }
            ended = true;
        }
        void SpawnBullet()
        {
            UnityEngine.GameObject b = pool.Allocate();
            var head = this.gameObject.transform.GetChild(0);
            b.transform.position = head.transform.position;
            b.transform.rotation = head.transform.rotation;
            
            
            // Calculate a random rotation offset within a specified range
            float randomAngle = Random.Range(-20, 20);
            // Apply the rotation offset to the bullet's rotation
            Quaternion randomRotation = Quaternion.Euler(randomAngle, randomAngle, 0);
            b.transform.rotation *= randomRotation;
            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
                gameObject, gameObject.GetComponent<ICanDealDamage>() , 50f);

            b.GetComponent<WormBullet>().SetData(bulletSpeed, player, bulletAccuracy);
        }

        public override void OnEnd()
        {
            base.OnEnd();
            StopAllCoroutines();
        }
    }
}
