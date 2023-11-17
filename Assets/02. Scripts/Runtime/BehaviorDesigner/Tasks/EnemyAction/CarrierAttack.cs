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


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class CarrierAttack : EnemyAction<QuadrupedCarrierEntity>
    {
        public SharedGameObject bulletPrefab;
        public GameObject muzzleFlash;
        
        //public int bulletCount;
        //public float spawnInterval;
        private bool ended;
        //public int bulletSpeed;
        public Transform shootPos;



        private Transform playerTrans;
        //public SharedTransform playerTrans;

        private SafeGameObjectPool pool;
        private SafeGameObjectPool pool2;
        private int bulletCount;
        private float spawnInterval;
        private float bulletSpeed;
        private float bulletAccuracy;
        private GameObject player;
        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(bulletPrefab.Value, 50, 100);
            pool2 = GameObjectPoolManager.Singleton.CreatePool(muzzleFlash, 50, 100);
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
                yield return new WaitForSeconds(spawnInterval);
            }
            ended = true;
        }

        void SpawnBullet(int bulletIndex, int totalBullets)
        {
            if (muzzleFlash != null)
            {
                
                muzzleFlash = pool2.Allocate();
                muzzleFlash.transform.position = (shootPos.position);
                


            }
            UnityEngine.GameObject b = pool.Allocate();
            b.transform.position = shootPos.position;
            b.transform.rotation = Quaternion.LookRotation(playerTrans.position -
                                                           new Vector3(transform.position.x, transform.position.y + 2,
                                                               transform.position.z));

            b.GetComponent<Rigidbody>().velocity = b.transform.forward * bulletSpeed;

            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
                gameObject, gameObject.GetComponent<ICanDealDamage>(), -1);
            b.GetComponent<CarrierBullet>().SetData(bulletSpeed);
            AudioSource audio = AudioSystem.Singleton.Play3DSound("Carrier_MachineGun", this.gameObject.transform.position);
            //AudioSystem.Singleton.StopSound(audio);
            


        }


        public override void OnEnd()
        {
            base.OnEnd();
            StopAllCoroutines();
        }
    }
}
