using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using UnityEngine;
using UnityEngine.AI;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class ShockWave : EnemyAction<BladeSentinelEntity>
    {
        private bool attackFinished = false;
        private BladeSentinel bossVC;
        private Transform playerTrans;

        public SharedGameObject shockWaveObj;
        
        private TaskStatus taskStatus;

        private int explosionDamage;
        private float explosionSize;

        private float jumpHeight;
        private float holdTime;
        private float chargeUpTime;
        private NavMeshAgent agent;
        public override void OnStart()
        {
            base.OnStart();
            bossVC = GetComponent<BladeSentinel>();
            playerTrans = GetPlayer().transform;
            explosionSize = enemyEntity.GetCustomDataValue<float>("shockWave", "explosionSize");
            explosionDamage = enemyEntity.GetCustomDataValue<int>("shockWave", "explosionDamage");
            jumpHeight = enemyEntity.GetCustomDataValue<float>("shockWave", "jumpHeight");
            chargeUpTime = enemyEntity.GetCustomDataValue<float>("shockWave", "chargeUpTime");
            holdTime = enemyEntity.GetCustomDataValue<float>("shockWave", "holdTime");
            taskStatus = TaskStatus.Running;
            StartCoroutine(DoAttack());
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = false;
            
        }
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        IEnumerator DoAttack()
        {
            anim.CrossFadeInFixedTime("Jump_Start", 0.2f);
            yield return new WaitForSeconds(chargeUpTime);
            anim.SetTrigger("Jump");
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"));

            //Jump
            float duration = 0.5f;
            float timeElapsed = 0;
            Vector3 startPosition = transform.position;
            
            //raycast up 
            RaycastHit hit;
            
            LayerMask mask = LayerMask.GetMask("Ground", "Wall");
            if (Physics.Raycast(transform.position, Vector3.up, out hit, jumpHeight + enemyViewController.SpawnSizeCollider.bounds.size.y
                    , mask, QueryTriggerInteraction.Ignore)) {
                jumpHeight = hit.point.y - transform.position.y - enemyViewController.SpawnSizeCollider.bounds.size.y;
            }

            Vector3 targetPosition = transform.position + new Vector3(0, jumpHeight, 0);
            while (timeElapsed < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;
            
            anim.CrossFade("GroundSlash_Attack",0f);
            yield return new WaitForSeconds(holdTime);
            
            //Crashdown
            
            var lookPos = playerTrans.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = rotation;
            duration = 0.5f;
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

            agent.enabled = true;
            GameObject shock = GameObject.Instantiate(shockWaveObj.Value);
            shock.transform.position = transform.position;
            shock.GetComponent<Example_Explosion>().Init(Faction.Explosion, explosionDamage, explosionSize,gameObject,
                gameObject.GetComponent<ICanDealDamage>());
            anim.SetTrigger("SlamAttackEnd");
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
            taskStatus = TaskStatus.Success;
        }

        public override void OnEnd() {
            base.OnEnd();
            agent.enabled = true;
            StopAllCoroutines();
        }
    }
}

