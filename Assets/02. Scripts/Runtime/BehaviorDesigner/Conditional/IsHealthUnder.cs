using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Temporary;
using UnityEngine;

namespace Runtime.BehaviorDesigner.Conditional
{
    public class IsEnemyHealthUnder : EnemyConditional {

        public SharedGameObject EntityGameObject;
        
        private IEnemyEntity enemyVC;
        public SharedFloat healthPercentage;
        public override void OnStart() {
            base.OnStart();
            enemyVC = EntityGameObject.Value.GetComponent<IEnemyViewController>().EnemyEntity;
        }

        public override TaskStatus OnUpdate() {

            int curHealth = enemyVC.HealthProperty.GetCurrentHealth();
            int maxHealth = enemyVC.HealthProperty.GetMaxHealth();
            if (curHealth < maxHealth * healthPercentage.Value) {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }



    }
}
