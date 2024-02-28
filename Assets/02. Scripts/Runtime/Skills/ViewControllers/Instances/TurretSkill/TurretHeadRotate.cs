using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class TurretHeadRotate : Action {
    [SerializeField] public SharedGameObject target;
    [SerializeField] public float rotationSpeed = 10f;
    public Transform source;

    public override TaskStatus OnUpdate() {
        if (target.Value == null) {
            return TaskStatus.Failure;
        }
        
        Vector3 direction = target.Value.transform.position - source.position;
        Vector3 forward = new Vector3(direction.x, 0, direction.z);

        float angle = Vector3.SignedAngle(forward, direction, Vector3.up);
        /*if (angle > 0) {
            angle = -angle;
        }*/
        if(direction.y > 0) {
            angle = -angle;
        }

        Quaternion rotation1 = source.rotation;
        Quaternion rotation = Quaternion.Euler(rotation1.eulerAngles.x,
            rotation1.eulerAngles.y, angle);
        
        
        source.rotation = Quaternion.Slerp(source.rotation, rotation, rotationSpeed * Time.deltaTime);
        
        if (Quaternion.Angle(source.rotation, rotation) < 0.5f) {
           // return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
}
