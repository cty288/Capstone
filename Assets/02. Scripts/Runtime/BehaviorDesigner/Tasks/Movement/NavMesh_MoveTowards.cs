using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.BehaviorDesigner.Tasks.Movement
{
    [TaskCategory("Unity/NavMeshAgent")]
    [TaskDescription("Like other MoveTowards but uses the NavMesh.")]
    public class NavMesh_MoveTowards : NavMeshMovement
    {
        [global::BehaviorDesigner.Runtime.Tasks.Tooltip("The agent has arrived when the magnitude is less than this value")]
        [UnityEngine.Serialization.FormerlySerializedAs("arriveDistance")]
        public SharedFloat m_ArriveDistance = 0.1f;
        [global::BehaviorDesigner.Runtime.Tasks.Tooltip("The GameObject that the agent is moving towards")]
        [UnityEngine.Serialization.FormerlySerializedAs("target")]
        public SharedGameObject m_Target;

        [global::BehaviorDesigner.Runtime.Tasks.Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;

        // cache the navmeshagent component
        private NavMeshAgent navMeshAgent;
        private GameObject prevGameObject;
        

        public override void OnStart()
        {
            var currentGameObject = GetDefaultGameObject(targetGameObject.Value);
            if (currentGameObject != prevGameObject)
            {
                navMeshAgent = currentGameObject.GetComponent<NavMeshAgent>();
                prevGameObject = currentGameObject;
            }
        }
        public override TaskStatus OnUpdate()
        {
            
            navMeshAgent.SetDestination(m_Target.Value.transform.position);

            // Return a task status of success once we've reached the target
            if (GetDistance() <= m_ArriveDistance.Value)
            {
                // Debug.Log("Success");
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }

        float GetDistance()
        {
            return Vector3.Distance(navMeshAgent.destination, navMeshAgent.gameObject.transform.position);
        }

        // Reset the public variables
        public override void OnReset()
        {
            m_ArriveDistance = 0.1f;
        }
        public override void OnEnd()
        {
            m_NavMeshAgent.ResetPath();
        }
    }
}
