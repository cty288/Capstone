using BehaviorDesigner.Runtime.Tasks;

namespace _02._Scripts.Runtime.Enemies.ViewControllers.Instances.WormBoss
{
    [TaskCategory("Unity/SharedVariable")]
    [TaskDescription("Sets the SharedColor variable to the specified object. Returns Success.")]
    public class SetSharedWormBossState : Action
    { 
        [Tooltip("The value to set the SharedColor to")]
        public SharedWormBossState targetValue;

        [RequiredField] [Tooltip("The SharedColor to set")]
        public SharedWormBossState targetVariable;

        public override TaskStatus OnUpdate()
        {
            targetVariable.Value = targetValue.Value;

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetValue = WormBossState.None;
            targetVariable = WormBossState.None;
        }
    }
}