using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class ShockWave : EnemyAction<BladeSentinelEntity>
    {
        private bool attackFinished = false;
        private BladeSentinel bossVC;
        private Transform playerTrans;

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
            anim.CrossFade("shockwave_jump",0f);
            yield return new WaitUntil(() => bossVC.currentAnimationEnd);
            bossVC.currentAnimationEnd = false;
            
            //Jump
            
            
            anim.CrossFade("shockwave_air",0f);
            yield return new WaitUntil(() => bossVC.currentAnimationEnd);
            bossVC.currentAnimationEnd = false;
            
            //Crashdown
            
            
        }
    }
}

