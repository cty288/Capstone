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
        public SharedFloat dashTime;
        private NavMeshAgent navMeshAgent;
        private float ogSpeed;
        private int currentCorner = 0;
        
        private NavMeshPath path = new NavMeshPath();
        private Vector3[] corners;
        private float timeStart;

        public override void OnStart()
        {
            rb = GetComponent<Rigidbody>();
            
            targetLocation = playerTrans.Value.position + transform.forward * 3;
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
            ogSpeed = navMeshAgent.speed;
            navMeshAgent.CalculatePath(targetLocation, path);
            corners = path.corners;
            
            timeStart = Time.time;
        }
        // public override TaskStatus OnUpdate()
        // {
        //     if (Time.time >= timeStart + dashTime.Value){
        //         // Debug.Log("Done");
        //         rb.velocity = Vector3.zero;
        //         navMeshAgent.speed = ogSpeed;
        //         return TaskStatus.Success;
        //     }
        //     else
        //     {
        //         Debug.Log("moving");
        //         if (corners != null && currentCorner < corners.Length)
        //         {
        //             Vector3 direction = corners[currentCorner] - transform.position;
        //             if (direction.magnitude < 0.5f)
        //             {
        //                 currentCorner++;
        //             }
        //             else
        //             {
        //                 rb.AddForce(direction.normalized * dashVelocity * 100 * Time.deltaTime);
        //             }
        //         }
        //         return TaskStatus.Running;
        //     }
        // }
        
        public override TaskStatus OnUpdate()
        {
            if (corners != null && currentCorner < corners.Length)
            {
                Vector3 direction = corners[currentCorner] - transform.position;
                if (direction.magnitude < 0.5f)
                {
                    currentCorner++;
                    return TaskStatus.Running;
                }
                else
                {
                    rb.AddForce(direction.normalized * dashVelocity * 20 * Time.deltaTime);
                    return TaskStatus.Running;
                }
            }
            return TaskStatus.Success;
        }
    }
}
