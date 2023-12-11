using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using UnityEngine;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class ShockWave : EnemyAction<BladeSentinelEntity>
    {
        private bool attackFinished = false;
        private BladeSentinel bossVC;
        private Transform playerTrans;

        public SharedGameObject shockWaveObj;

        [SerializeField] private int explosionDamage;
        [SerializeField] private float explosionSize;

        [SerializeField] private float jumpHeight;
        public override void OnStart()
        {
            base.OnStart();
            bossVC = GetComponent<BladeSentinel>();
            playerTrans = GetPlayer().transform;
            StartCoroutine(DoAttack());
        }
        public override TaskStatus OnUpdate()
        {

            return TaskStatus.Running;
        }

        IEnumerator DoAttack()
        {
            anim.CrossFadeInFixedTime("Shockwave_Jump", 0.2f);
            yield return new WaitUntil(() => bossVC.currentAnimationEnd);
            bossVC.currentAnimationEnd = false;

            //Jump
            float duration = 1;
            float timeElapsed = 0;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = transform.position + new Vector3(0, jumpHeight, 0);
            while (timeElapsed < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;
            
            
            
            anim.CrossFade("Shockwave_Air",0f);
            yield return new WaitUntil(() => bossVC.currentAnimationEnd);
            bossVC.currentAnimationEnd = false;
            
            
            //Crashdown
            duration = 1;
            timeElapsed = 0;
            startPosition = transform.position;
            targetPosition = playerTrans.position;
            while (timeElapsed < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;

            GameObject shock = GameObject.Instantiate(shockWaveObj.Value);
            shock.transform.position = transform.position;
            shock.GetComponent<Example_Explosion>().Init(Faction.Neutral, explosionDamage, explosionSize,gameObject,
                gameObject.GetComponent<ICanDealDamage>());
        }
    }
}

