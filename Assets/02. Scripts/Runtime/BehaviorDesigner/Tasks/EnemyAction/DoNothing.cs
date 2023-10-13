using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class DoNothing : EnemyAction
    {
       
        public override void OnStart()
        {
         

        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Running;

        }

        // Implement the MoveUnderground method to move the cactus underground.
       
    }
}

