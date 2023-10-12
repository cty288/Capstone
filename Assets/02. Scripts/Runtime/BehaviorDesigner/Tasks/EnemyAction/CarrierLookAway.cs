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
using UnityEngine.Animations.Rigging;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class CarrierLookAway : EnemyAction<QuadrupedCarrierEntity>
    {

        private bool ended;
        //public int bulletSpeed;
        public GameObject rigObject;
        private RigBuilder rb;
        public MultiAimConstraint mac;



        private Transform playerTrans;
        //public SharedTransform playerTrans;

        private SafeGameObjectPool pool;
        private int bulletCount;
        private float spawnInterval;
        private float bulletSpeed;
        private float bulletAccuracy;
        private GameObject player;
        private float timer = 2f;
        public override void OnAwake()
        {
            base.OnAwake();
            playerTrans = GetPlayer().transform;
            player = GetPlayer();
        }

        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            rb = rigObject.GetComponent<RigBuilder>();
            mac = GameObject.FindObjectOfType<MultiAimConstraint>();



            StartCoroutine(DecreaseWeight());
        }
        public override TaskStatus OnUpdate()
        {
            if (ended)
                return TaskStatus.Success;
            else
                return TaskStatus.Running;

        }
        IEnumerator DecreaseWeight()
        {
            float elapsedTime = 0;
            float startWeight = 1;
            float endWeight = 0;

            while (elapsedTime < timer)
            {
                float t = elapsedTime / timer;
                mac.weight = Mathf.Lerp(startWeight, endWeight, t);

                yield return null; // Wait for the next frame
                elapsedTime += Time.deltaTime;
            }

            // Ensure the weight is exactly 1 at the end
            mac.weight = endWeight;
            ended = true;
        }



        public override void OnEnd()
        {
            base.OnEnd();

            timer = 2f;
            StopAllCoroutines();
        }
    }
}
