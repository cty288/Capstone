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
    public class WormUnderGround : EnemyAction<HunterWormEntity>
    {
        NavMeshAgent agent;
        public BehaviorTree tree;
        float elapsedTime;
        bool done;
        public SharedGameObject player;
        public override void OnAwake()
        {
            base.OnAwake();
            agent = this.gameObject.GetComponent<NavMeshAgent>();

        }

        public override void OnStart()
        {
            StartCoroutine(ChangeOffset());
            tree = this.gameObject.GetComponent<BehaviorTree>();
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
            while (elapsedTime < 0.3f)
            {
                // Interpolate the offset value over time.
                float t = elapsedTime / 0.3f;
                float newOffset = Mathf.Lerp(0.5f, 0.15f, t);

                // Set the new offset value for the NavMeshAgent.
                agent.baseOffset = newOffset;

                // Update elapsed time.
                elapsedTime += Time.deltaTime;
                if(player.Value != null)
                {
                    var position = player.Value.transform.position;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(position - transform.position), 360f);
                }
                

                yield return null;
            }
            done = true;
        }



        public override void OnEnd()
        {
            SharedBool isGround = (SharedBool)tree.GetVariable("InGround");
            isGround = true;
            base.OnEnd();
            StopAllCoroutines();
            done = false;
            elapsedTime = 0;
        }
    }
}
