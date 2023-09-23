using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class ChangeShellStatus : EnemyAction
    {
        public Boss1 bossVC;
        public SharedBool closed;
        public override void OnStart()
        {
            bossVC.ChangeShellStatus(closed.Value);
        }
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }

}
