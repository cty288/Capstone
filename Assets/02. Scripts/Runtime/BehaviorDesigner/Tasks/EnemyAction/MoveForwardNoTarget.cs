using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;
namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class MoveForwardNoTarget : EnemyAction
    {
        public SharedGameObject target;
        private Rigidbody rb;
        public float speed;
        public float arrivalDistanceThreshold;
        private Transform body;

        public override void OnStart()
        {
            // this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            base.OnStart();
            rb = this.GetComponent<Rigidbody>();
            body = this.gameObject.transform.GetChild(0);
        }

        public override TaskStatus OnUpdate()
        {
            var position = GetPlayer().transform.position;
            var dir = (position - body.transform.position).normalized;
            rb.velocity = dir * speed;
            this.gameObject.transform.LookAt(position);
            // Check if the distance between the object and the target is less than the threshold
            if (Vector3.Distance(transform.position, target.Value.transform.position) < 2)
            {
                this.gameObject.GetComponent<NavMeshAgent>().enabled = true;
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}