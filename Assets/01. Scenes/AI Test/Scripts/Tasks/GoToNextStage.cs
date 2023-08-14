using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
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