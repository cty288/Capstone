using _02._Scripts.Runtime.Enemies.ViewControllers.Instances.WormBoss;
using BehaviorDesigner.Runtime.Tasks;

namespace _02._Scripts.Runtime.BehaviorDesigner.Tasks.EnemyAction.WormBoss
{
    [TaskCategory("Unity/SharedVariable")]
    [TaskDescription("Returns success if the variable value is equal to the compareTo value.")]
    public class CompareSharedWormBossState : Conditional
    {
        [Tooltip("The first variable to compare")]
        public SharedWormBossState variable;
        [Tooltip("The variable to compare to")]
        public SharedWormBossState compareTo;

        public override TaskStatus OnUpdate()
        {
            return variable.Value.Equals(compareTo.Value) ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            variable = WormBossState.None;
            compareTo = WormBossState.None;
        }
    }
}