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
    public class CarrierAttack : EnemyAction<QuadrupedCarrierEntity>
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
                SpawnBullet(i, bulletCount);
                yield return null;
            }
            ended = true;
        }

        void SpawnBullet(int bulletIndex, int totalBullets)
        {
            UnityEngine.GameObject b = pool.Allocate();

            // Calculate the angle between each bullet
            float angleStep = 360f / totalBullets;

            // Calculate the angle for the current bullet
            float angle = bulletIndex * angleStep;

            // Convert the angle to radians
            float angleRad = angle * Mathf.Deg2Rad;

            // Specify the radius of the circle
            float radius = 1.0f; // You can adjust the radius as needed

            // Calculate the position for the bullet in a circle
            Vector3 spawnPosition = gameObject.transform.position +
                new Vector3(Mathf.Cos(angleRad) * radius, 0f, Mathf.Sin(angleRad) * radius);

            b.transform.position = spawnPosition;
            b.transform.rotation = gameObject.transform.rotation;

            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
                gameObject, gameObject.GetComponent<ICanDealDamage>(), 50f);

            b.GetComponent<CarrierBullet>().SetData(bulletSpeed);
        }


        public override void OnEnd()
        {
            base.OnEnd();
            StopAllCoroutines();
        }
    }
}
