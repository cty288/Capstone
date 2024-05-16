using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework.BindableProperty;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Enemies;
using Runtime.Enemies.Model.Properties;
using UnityEngine;


namespace Runtime.BehaviorDesigner.Tasks
{
    public class ShellRegen : EnemyAction<Boss1Entity>
    {
        // Start is called before the first frame update
        public override void OnStart()
        {
            base.OnStart();
            IBindableProperty shellHp = enemyEntity.GetCustomDataValue("shellHealthInfo", "info");
            shellHp.Value = new HealthInfo(shellHp.Value.MaxHealth,shellHp.Value.MaxHealth);
            
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}

