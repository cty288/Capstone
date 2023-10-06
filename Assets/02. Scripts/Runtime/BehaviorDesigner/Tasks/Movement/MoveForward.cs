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
    public class MoveForward : Action
    {
        public SharedGameObject target;
        private Rigidbody rb;
        public float speed;
        public float arrivalDistanceThreshold;

        public override void OnStart()
        {
            this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            base.OnStart();
            rb = this.GetComponent<Rigidbody>();
        }

        public override TaskStatus OnUpdate()
        {
            var position = target.Value.transform.position;
            var dir = (position - this.gameObject.transform.position).normalized;
            rb.velocity = dir * speed;
            Debug.Log(Vector3.Distance(transform.position, position));
            // Check if the distance between the object and the target is less than the threshold
            if (Vector3.Distance(transform.position, target.Value.transform.position) < arrivalDistanceThreshold)
            {
                this.gameObject.GetComponent<NavMeshAgent>().enabled = true;
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}
