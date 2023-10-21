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
using UnityEngine.AI;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormReturnGround : EnemyAction<HunterWormEntity>
    {
        NavMeshAgent agent;
        private float timer = 1.5f;
        public BehaviorTree tree;
        private float elapsedTime;
        bool done;
        public override void OnAwake()
        {
            base.OnAwake();
            agent = this.gameObject.GetComponent<NavMeshAgent>();

        }

        public override void OnStart()
        {
            //agent.baseOffset = 3.5f;
            tree = this.gameObject.GetComponent<BehaviorTree>();
            StartCoroutine(ChangeOffset());
        }
        public override TaskStatus OnUpdate()
        {
            if (done)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Running;
            }

        }

        private IEnumerator ChangeOffset()
        {
            while(elapsedTime < 1.5f)
            {
                // Interpolate the offset value over time.
                float t = elapsedTime / 1.5f;
                float newOffset = Mathf.Lerp(1.25f, 3.5f, t);

                // Set the new offset value for the NavMeshAgent.
                agent.baseOffset = newOffset;

                // Update elapsed time.
                elapsedTime += Time.deltaTime;

                yield return null;
            }
            done = true;
        }
        public override void OnEnd()
        {
            SharedBool isGround = (SharedBool)tree.GetVariable("InGround");
            isGround = false;
            base.OnEnd();
            StopAllCoroutines();
            done = false;
            elapsedTime = 0;
        }
    }
}
