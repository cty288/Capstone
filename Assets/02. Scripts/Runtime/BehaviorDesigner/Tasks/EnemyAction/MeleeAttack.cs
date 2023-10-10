using System.Collections;
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

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class MeleeAttack : EnemyAction<Boss1Entity>
    {

        
        //public SharedTransform playerTrans;

        public override void OnAwake() {
            base.OnAwake();
        }

        public override void OnStart()
        {
            base.OnStart();
            anim.SetTrigger("Attack");
        }
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;

        }


    }
}
