using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class TurretRotateTowards : Action
{
    
        [UnityEngine.Tooltip("The agent is done rotating when the angle is less than this value")]
        [UnityEngine.Serialization.FormerlySerializedAs("rotationEpsilon")]
        public SharedFloat m_RotationEpsilon = 0.5f;

        public float RotateSpeed = 20f;
      
      
        [UnityEngine.Tooltip("The GameObject that the agent is rotating towards")]
        [UnityEngine.Serialization.FormerlySerializedAs("target")]
        public SharedGameObject m_Target;
        

        public Transform source;
        public Transform rotateSource;

        
        
        public override void OnStart() {
            base.OnStart();
           
            
        }
        
        

        public override TaskStatus OnUpdate()
        {
            if (!source) {
                source =  transform;
            }
            
          
            Vector3 direction = m_Target.Value.transform.position - source.position;
            Vector3 lookDirection = -source.right;
            float a = Vector3.SignedAngle(lookDirection, direction, Vector3.up);
            if (Mathf.Abs(a) < m_RotationEpsilon.Value) {
                return TaskStatus.Success;
            }


            Vector3 originalRotation = rotateSource.rotation.eulerAngles;
            Vector3 targetRotation = new Vector3(originalRotation.x, originalRotation.y + a
                , originalRotation.z);
            rotateSource.rotation = Quaternion.Lerp(rotateSource.rotation, Quaternion.Euler(targetRotation),
                RotateSpeed * Time.deltaTime);
            
            
            return TaskStatus.Running;
        }

        /*
        // Return targetPosition if targetTransform is null
        private float GetRotateAngle() {
            Vector3 direction = m_Target.Value.transform.position - source.position;
            float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            return angleY;
            
        }
        */

        // Reset the public variables
        public override void OnReset()
        {
           
            m_RotationEpsilon = 0.5f;
            m_Target = null;
        }
}
