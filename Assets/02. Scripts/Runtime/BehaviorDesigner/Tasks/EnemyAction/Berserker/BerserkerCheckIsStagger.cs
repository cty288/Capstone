using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.BehaviorDesigner.Conditional;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using UnityEngine;

namespace _02._Scripts.Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class BerserkerCheckIsStagger : EnemyConditional<BerserkerEntity>
    {
        public override TaskStatus OnUpdate()
        {
            if (!enemyEntity.IsStaggered && enemyEntity.StaggerDamage >= enemyEntity.StaggerThreshold)
            {
                Debug.Log($"BERSERKER: stagger = true");
                
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}