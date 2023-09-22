using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.Temporary;
using UnityEngine;
using Runtime.Enemies;
using Runtime.Utilities.Collision;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class RangedAOE : EnemyAction
    {
        public Boss1 bossVC;
        public SharedGameObject bulletPrefab;
        public int bulletCount;
        public float spawnInterval;
        private bool ended;
        public float bulletTravelTime;

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
            for (int i = 0; i < bulletCount; i++)
            {
                SpawnBullet();
                yield return new WaitForSeconds(spawnInterval);
            }
            ended = true;
        }
        void SpawnBullet()
        {
            GameObject b = Object.Instantiate(bulletPrefab.Value, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), Quaternion.identity);
            b.GetComponent<EnemyBomb>().Init(playerTrans.Value, bulletTravelTime);
            //HitBox bHitBox = b.GetComponent<HitBox>();
            //bHitBox.HitResponder = bossVC;
            //bHitBox.StartCheckingHits();
            // Debug.Log("b.GetComponent<HitBox>().HitResponder: " + bossVC);
            b.GetComponent<Temp_BulletHitResponder>().boss1 = bossVC.gameObject;

        }
    }
}
