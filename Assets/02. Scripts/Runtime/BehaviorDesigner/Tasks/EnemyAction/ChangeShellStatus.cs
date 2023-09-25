using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class ChangeShellStatus : EnemyAction<Boss1Entity>
    {
       
        public SharedBool closed;
        public override void OnStart() {
            enemyEntity.ChangeShellStatus(closed.Value);
        }
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }

}
