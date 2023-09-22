using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class Stun : EnemyAction
    {
        private Transform targetObject; // The object you want to rotate toward
        private float rotationDuration = 2.0f; // Duration of the rotation in seconds

        private Quaternion initialRotation; // The initial rotation of your object
        private Quaternion targetRotation; // The target rotation
        public float duration;
        public GameObject pivot;
        float elapsedTime = 0.0f;
        public override void OnStart()
        {
             elapsedTime = 0.0f;
            initialRotation = pivot.transform.rotation;
            targetRotation = Quaternion.Euler(0, 0, 0);
            
            
        }



        public override TaskStatus OnUpdate()
        {
            
            if(elapsedTime < rotationDuration)
            {
                Debug.Log(elapsedTime);
                pivot.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / rotationDuration);
                elapsedTime += Time.deltaTime;

                return TaskStatus.Running;
            }
            else
            {
                transform.rotation = targetRotation; // Ensure the final 
                return TaskStatus.Success;
            }

           
              
            
         
        }
    }
}
