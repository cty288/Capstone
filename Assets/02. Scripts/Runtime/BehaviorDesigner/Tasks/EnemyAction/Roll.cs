using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class Roll : EnemyAction
    {


        public SharedTransform playerTrans;
        Rigidbody rb;
        private Vector3 targetLocation;
        public int dashVelocity;
        private NavMeshAgent navMeshAgent;
        private float ogSpeed;

        public override void OnStart()
        {
        
            rb = GetComponent<Rigidbody>();
            targetLocation = playerTrans.Value.position + transform.forward * 3;
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
            ogSpeed = navMeshAgent.speed;
        }
        public override TaskStatus OnUpdate()
        {
            if (Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(targetLocation
                    .x,targetLocation.z)) <= 0.5f){
                navMeshAgent.speed = ogSpeed;
                return TaskStatus.Success;
            }
            else
            {
                navMeshAgent.speed = dashVelocity;
                navMeshAgent.SetDestination(targetLocation);
                return TaskStatus.Running;
            }
        
        }
    }
}
