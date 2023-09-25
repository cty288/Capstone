using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class RapidFire : EnemyAction<Boss1Entity>
    {
        public SharedGameObject bulletPrefab;
        public int bulletCount;
        public float spawnInterval;
        private bool ended;
        public int bulletSpeed;
        public Boss1 boss1vc;

        public SharedTransform playerTrans;

        public override void OnStart()
        {
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
            for(int i = 0; i < bulletCount; i++)
            {
                SpawnBullet();
                yield return new WaitForSeconds(spawnInterval);
            }
            ended = true;
        }
        void SpawnBullet() {
            GameObject b = Object.Instantiate(bulletPrefab.Value,
                new Vector3(transform.position.x, transform.position.y + 2, transform.position.z),
                Quaternion.LookRotation(playerTrans.Value.position -
                                        new Vector3(transform.position.x, transform.position.y + 2,
                                            transform.position.z)));
            
            b.GetComponent<Rigidbody>().velocity = b.transform.forward * bulletSpeed;
            // HitBox bHitBox = b.GetComponent<HitBox>();
            // bHitBox.HitResponder = boss1vc;
            // bHitBox.StartCheckingHits();
            // Debug.Log("b hit responder: " + b.GetComponent<Temp_BulletHitResponder>());
            // Debug.Log("boss1: " + b.GetComponent<Temp_BulletHitResponder>().boss1);
            // Debug.Log("boss1vc: " + boss1vc);
            

            b.GetComponent<Temp_BulletHitResponder>().boss1 = boss1vc.gameObject;

        }
    }
}
