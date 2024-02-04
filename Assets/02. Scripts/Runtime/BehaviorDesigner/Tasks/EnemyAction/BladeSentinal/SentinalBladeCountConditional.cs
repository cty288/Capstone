using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using UnityEngine;

public class SentinalBladeCountConditional : EnemyAction<BladeSentinelEntity> {
    public enum Operation
    {
        LessThan,
        LessThanOrEqualTo,
        EqualTo,
        NotEqualTo,
        GreaterThanOrEqualTo,
        GreaterThan
    }

    [BehaviorDesigner.Runtime.Tasks.Tooltip("The operation to perform: bladeCount [operation] variableToCompare")]
    public Operation operation;

    [BehaviorDesigner.Runtime.Tasks.Tooltip("The second float")]
    public SharedInt variableToCompare;

    public override TaskStatus OnUpdate()
    {
        switch (operation) {
            case Operation.LessThan:
                return enemyEntity.GetCurrentBladeCount() < variableToCompare.Value ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.LessThanOrEqualTo:
                return enemyEntity.GetCurrentBladeCount() <= variableToCompare.Value ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.EqualTo:
                return enemyEntity.GetCurrentBladeCount() == variableToCompare.Value ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.NotEqualTo:
                return enemyEntity.GetCurrentBladeCount() != variableToCompare.Value ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.GreaterThanOrEqualTo:
                return enemyEntity.GetCurrentBladeCount() >= variableToCompare.Value ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.GreaterThan:
                return enemyEntity.GetCurrentBladeCount() > variableToCompare.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
        return TaskStatus.Failure;
    }

    public override void OnReset()
    {
        operation = Operation.GreaterThanOrEqualTo;
        variableToCompare.Value = 0;
    }
}
