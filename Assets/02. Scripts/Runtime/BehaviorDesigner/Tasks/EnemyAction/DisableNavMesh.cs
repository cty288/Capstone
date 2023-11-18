using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Threading.Tasks;
using Runtime.Enemies.SmallEnemies;
using UnityEngine.AI;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class DisableNavMesh : EnemyAction<BeeEntity>
    {
        public override void OnStart()
        {
            base.OnStart();
            this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}