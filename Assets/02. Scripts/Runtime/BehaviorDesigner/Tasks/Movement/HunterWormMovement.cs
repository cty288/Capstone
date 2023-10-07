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
        public float rayLength = 1000f;
        public LayerMask layerMask = 1 << 8; // Layer 8

        public override void OnStart()
        {
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
                transform.position = hit.point;
                return TaskStatus.Running;
            }

            // If the ray doesn't hit anything on the specified layer, return failure
            return TaskStatus.Failure;
        }
    }
}
