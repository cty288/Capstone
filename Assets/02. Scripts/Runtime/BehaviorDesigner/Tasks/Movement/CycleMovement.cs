
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using UnityEngine;


namespace Runtime.BehaviorDesigner.Tasks.Movement
{
    [TaskDescription("Move around the area in a circle")]
    [TaskCategory("Movement")]
    public class CycleMovement : NavMeshMovement
    {
        public SharedFloat radius = 5f;
        public SharedFloat speed = 2f;
        

        private Vector3 originalPosition;
        private float elapsedTime = 0f;
        private Vector3 tempVector;

        public override void OnStart()
        {
            base.OnStart();
            originalPosition = transform.position;
            elapsedTime = 0f;
        }

        public override TaskStatus OnUpdate()
        {
            tempVector.Set(Mathf.Cos(elapsedTime * speed.Value) * radius.Value,
                0f,
                Mathf.Sin(elapsedTime * speed.Value) * radius.Value);
            Vector3 newPosition = originalPosition + tempVector;

            SetDestination(newPosition);

            elapsedTime += Time.deltaTime;

            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            //transform.position = originalPosition;
            elapsedTime = 0f;
        }

        public override void OnReset()
        {
            base.OnReset();
            radius = 5f;
            speed = 2f;
        }
    }
}