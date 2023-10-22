using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;
using Runtime.Enemies.SmallEnemies;
using UnityEngine.AI;

namespace Runtime.BehaviorDesigner.Tasks.Movement
{
    public class HunterWormMovement : Action
    {
        NavMeshAgent agent;
        public Transform head;
        public float rotationSpeed = 5.0f; // Adjust the speed as needed
        public float bodyRotationSpeed = 20.0f; // Adjust the speed as needed
        public bool headSpin;
        private Quaternion targetRotation;
        public SharedGameObject m_Target;
        public override void OnStart()
        {
            base.OnStart();
            agent = this.gameObject.GetComponent<NavMeshAgent>();
            agent.updateRotation = false;

        }
        public override TaskStatus OnUpdate()
        {
            if (headSpin)
            {
                var head = this.gameObject.transform.GetChild(0);
                float rotationSpeed = 70f; 

               
                head.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            }
            return TaskStatus.Running;
        }

        public override void OnLateUpdate()
        {
            // Calculate the target rotation based on the agent's velocity
            Vector3 targetDirection = agent.velocity.normalized;
            if (targetDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(targetDirection);
            }

            var bodyRotation = transform.rotation;
            bodyRotation = Quaternion.Slerp(bodyRotation, Quaternion.RotateTowards(bodyRotation,
                Quaternion.LookRotation(m_Target.Value.transform.position - transform.position), 360), bodyRotationSpeed * Time.deltaTime);
            transform.rotation = bodyRotation;
            

            // Smoothly rotate the head
            head = this.gameObject.transform.GetChild(0);
            Debug.Log(targetDirection);
            head.transform.rotation = Quaternion.Slerp(head.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        /*
        public float rayLength = 1000f;
        public LayerMask layerMask = 1 << 8; // Layer 8
        private BehaviorTree tree;

        public override void OnStart()
        {
            tree = this.gameObject.GetComponent<BehaviorTree>();
            this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            base.OnStart();
           
        }

        public override TaskStatus OnUpdate()
        {
            // Create a ray starting from above the GameObject
            Ray ray = new Ray(transform.position + new Vector3(0, 100, 0), Vector3.down);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayLength, layerMask))
            {
                // If the ray hits an object on the specified layer, move the GameObject to the hit point
                SharedVector3 initialPosition = (SharedVector3)tree.GetVariable("InitialPosition");
                initialPosition.SetValue(this.transform.position + new Vector3(0,1,0));
                transform.position = hit.point;
                return TaskStatus.Running;
            }

            // If the ray doesn't hit anything on the specified layer, return failure
            return TaskStatus.Failure;
        }
        */
    }
}
