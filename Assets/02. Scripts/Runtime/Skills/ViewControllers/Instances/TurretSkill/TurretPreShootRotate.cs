using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class TurretPreShootRotate : Action {
   public SharedFloat RotateSpeed;
   public SharedFloat StopRotateAngle;
   public SharedGameObject Target;
   public Transform HeadTransform;

   public override TaskStatus OnUpdate() {
      if(Target.Value == null || Target.Value.activeSelf == false) {
         return TaskStatus.Failure;
      }
      
      //rotate head to the target
      var targetPosition = Target.Value.transform.position;
      var targetDirection = targetPosition - HeadTransform.position;
      var newRotation = Quaternion.LookRotation(targetDirection);
      var newEuler = newRotation.eulerAngles;
      newEuler.z = 0;
      newRotation = Quaternion.Euler(newEuler);
      HeadTransform.rotation = Quaternion.Slerp(HeadTransform.rotation, newRotation, RotateSpeed.Value * Time.deltaTime);
      
      //if the angle between the head and the target is less than the stop rotate angle, return success
      if(Quaternion.Angle(HeadTransform.rotation, newRotation) < StopRotateAngle.Value) {
         return TaskStatus.Success;
      }
      
      return TaskStatus.Running;
   }
}
