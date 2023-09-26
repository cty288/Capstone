using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies.SmallEnemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
   
    public class FireBullet : EnemyAction
    {
        public SharedGameObject bulletPrefab;
        // Start is called before the first frame update


        public override TaskStatus OnUpdate()
        {
            Debug.Log("shooting");

            return TaskStatus.Success;
        }
    }
}
