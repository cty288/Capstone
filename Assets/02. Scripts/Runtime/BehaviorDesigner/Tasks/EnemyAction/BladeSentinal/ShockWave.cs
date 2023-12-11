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
        
        private TaskStatus taskStatus;

        [SerializeField] private int explosionDamage;
        [SerializeField] private float explosionSize;

        [SerializeField] private float jumpHeight;
        [SerializeField] private float holdTime;
        public override void OnStart()
        {
            base.OnStart();
            bossVC = GetComponent<BladeSentinel>();
            playerTrans = GetPlayer().transform;
            taskStatus = TaskStatus.Running;
            StartCoroutine(DoAttack());
            
        }
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        IEnumerator DoAttack()
        {
            anim.CrossFadeInFixedTime("Jump_Start", 0.2f);
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"));

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

            yield return new WaitForSeconds(holdTime);
            
            anim.CrossFade("GroundSlash_Attack",0f);
            
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
            yield return new WaitForSeconds(3f);
            taskStatus = TaskStatus.Success;
        }
    }
}

