using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks.Movement;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
    [TaskDescription("Move around the area in a circle")]
    [TaskCategory("Movement")]
    public class CycleMovement : NavMeshMovement
    {
        public SharedFloat radius = 5f;
        public SharedFloat speed = 2f;

        private Vector3 originalPosition;
        private float elapsedTime = 0f;

        public override void OnStart()
        {
            base.OnStart();
            originalPosition = transform.position;
            elapsedTime = 0f;
        }

        public override TaskStatus OnUpdate()
        {
            Vector3 newPosition = originalPosition +
                                  new Vector3(Mathf.Cos(elapsedTime * speed.Value) * radius.Value,
                                              0f,
                                              Mathf.Sin(elapsedTime * speed.Value) * radius.Value);

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
