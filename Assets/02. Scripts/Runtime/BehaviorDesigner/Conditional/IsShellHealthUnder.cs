using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies;
using Runtime.Enemies.Model;
using Runtime.Temporary;
using UnityEngine;
namespace Runtime.BehaviorDesigner.Conditional
{
    public class IsShellHealthUnder : EnemyConditional<Boss1Entity>
    {

        
        public SharedFloat healthPercentage;
        private IBindableProperty shellHp;
        public override void OnStart() {
            base.OnStart();
            shellHp = enemyEntity.GetCustomDataValue("shellHealthInfo", "info");
        }

        public override TaskStatus OnUpdate() {


            if (shellHp.Value.CurrentHealth < shellHp.Value.MaxHealth* healthPercentage.Value && enemyEntity.ShellClosed.Value) {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }



    }
}
