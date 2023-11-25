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
    public class WormAttackToxicFire : EnemyAction<HunterWormEntity>
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
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 30, 50);
            playerTrans = GetPlayer().transform;
            player = GetPlayer();
            //Faction faction = enemyEntity.CurrentFaction;
        }

        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            spawnInterval = enemyEntity.GetCustomDataValue<float>("attack", "toxicSpawnInterval");
            bulletCount = enemyEntity.GetCustomDataValue<int>("attack", "toxicAmmoSize");

            bulletSpeed = enemyEntity.GetCustomDataValue<float>("attack", "toxicBulletSpeed");
            
            
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
            ended = true;
        }
        void SpawnBullet()
        {
            UnityEngine.GameObject b = pool.Allocate();
            var head = this.gameObject.transform.GetChild(0);
            b.transform.position = head.transform.position;
            b.transform.rotation = head.transform.rotation;
            b.transform.LookAt(playerTrans);

            
            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
                gameObject, gameObject.GetComponent<ICanDealDamage>(), 50f);

            b.GetComponent<WormBulletToxic>().SetData(bulletSpeed);
        }

        public override void OnEnd()
        {
            base.OnEnd();
            StopAllCoroutines();
        }
    }
}
