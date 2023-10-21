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
        public BehaviorTree tree;
        public override void OnAwake()
        {
            base.OnAwake();
            agent = this.gameObject.GetComponent<NavMeshAgent>();

        }

        public override void OnStart()
        {
            agent.baseOffset = 3.5f;
            tree = this.gameObject.GetComponent<BehaviorTree>();
        }
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;

        }



        public override void OnEnd()
        {
            SharedBool isGround = (SharedBool)tree.GetVariable("InGround");
            isGround = false;
            base.OnEnd();
            StopAllCoroutines();
        }
    }
}
