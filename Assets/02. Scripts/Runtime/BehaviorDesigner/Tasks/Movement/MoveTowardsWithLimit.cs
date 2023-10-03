using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Runtime.BehaviorDesigner.Tasks.Movement
{
    [TaskDescription("Move towards the specified position with certain range limit. The position can either be specified by a transform or position. If the transform " +
                     "is used then the position will not be used.")]
    [TaskCategory("Movement")]
    [TaskIcon("c8e612848487a184f9090d416c932c47", "812dc79fe1e417548959f61845528372")]
    public class MoveTowardsWithLimit : Action
    {
        [global::BehaviorDesigner.Runtime.Tasks.Tooltip("The speed of the agent")]
        [UnityEngine.Serialization.FormerlySerializedAs("speed")]
        public SharedFloat m_Speed;
        [global::BehaviorDesigner.Runtime.Tasks.Tooltip("The agent has arrived when the magnitude is less than this value")]
        [UnityEngine.Serialization.FormerlySerializedAs("arriveDistance")]
        public SharedFloat m_ArriveDistance = 0.1f;
        [global::BehaviorDesigner.Runtime.Tasks.Tooltip("Should the agent be looking at the target position?")]
        [UnityEngine.Serialization.FormerlySerializedAs("lookAtTarget")]
        public SharedBool m_LookAtTarget = true;
        [global::BehaviorDesigner.Runtime.Tasks.Tooltip("Max rotation delta if lookAtTarget is enabled")]
        [UnityEngine.Serialization.FormerlySerializedAs("maxLookAtRotationDelta")]
        public SharedFloat m_MaxLookAtRotationDelta;
        [global::BehaviorDesigner.Runtime.Tasks.Tooltip("The GameObject that the agent is moving towards")]
        [UnityEngine.Serialization.FormerlySerializedAs("target")]
        public SharedGameObject m_Target;
        [global::BehaviorDesigner.Runtime.Tasks.Tooltip("If target is null then use the target position")]
        [UnityEngine.Serialization.FormerlySerializedAs("targetPosition")]
        public SharedVector3 m_TargetPosition;
        public float limit;

        public override TaskStatus OnUpdate()
        {
            var position = Target();

            transform.position = Vector3.MoveTowards(transform.position, position, m_Speed.Value * Time.deltaTime);
            if (m_LookAtTarget.Value && (position - transform.position).sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(position - transform.position), m_MaxLookAtRotationDelta.Value);
            }
            if (Vector3.Magnitude(transform.position - position) < m_ArriveDistance.Value)
            {
                Debug.Log("running2");
                return TaskStatus.Running;
            }
         
            if (Vector3.Magnitude(transform.position - position) > limit)
            {
                Debug.Log("running3");
                return TaskStatus.Success;
            }
            // Return a task status of success once we've reached the target


            // We haven't reached the target yet so keep moving towards it
            //transform.position = Vector3.MoveTowards(transform.position, position, m_Speed.Value * Time.deltaTime);

            Debug.Log("running");
            return TaskStatus.Running;
        }

        // Return targetPosition if targetTransform is null
        private Vector3 Target()
        {
            if (m_Target == null || m_Target.Value == null)
            {
                return m_TargetPosition.Value;
            }
            return m_Target.Value.transform.position;
        }

        // Reset the public variables
        public override void OnReset()
        {
            m_ArriveDistance = 0.1f;
            m_LookAtTarget = true;
        }
    }
}