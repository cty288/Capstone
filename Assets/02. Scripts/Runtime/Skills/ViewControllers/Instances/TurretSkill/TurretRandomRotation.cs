using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class TurretRandomRotation : Action
{
    public SharedVector3 returnRotation;

    public override TaskStatus OnUpdate() {
        returnRotation.Value = new Vector3(0, Random.Range(0, 360), 0);
        return TaskStatus.Success;
    }
}
