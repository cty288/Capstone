using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class SuccessAction : Action
{
    public override TaskStatus OnUpdate() {
        return TaskStatus.Success;
    }
}
