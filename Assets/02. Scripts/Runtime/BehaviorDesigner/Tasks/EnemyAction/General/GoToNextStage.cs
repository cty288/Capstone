using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class GoToNextStage : EnemyAction
    {
        public SharedInt currentStage;

        public override TaskStatus OnUpdate()
        {
            currentStage.Value++;
            return TaskStatus.Success;
        }
    }
}