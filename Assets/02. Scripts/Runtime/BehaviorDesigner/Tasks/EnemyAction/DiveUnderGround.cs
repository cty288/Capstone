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

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class DiveUnderGround : EnemyAction
    {
        public SharedTransform headPosition;
        public float moveSpeed = 1.0f;
        public float maxDistanceToGround = 2.0f;
        public float timer = 1f;
        private bool isMovingUnderground;
        private bool finished;
        public SharedBool underGround;
        public ReturnToGround rtg;
        public BehaviorTree tree;
        

        public override void OnStart()
        {
            isMovingUnderground = false;
            tree = this.gameObject.GetComponent<BehaviorTree>();
            SharedVector3 initialPosition = (SharedVector3)tree.GetVariable("InitialPosition");
            initialPosition.SetValue(this.transform.position);

        }

        public override TaskStatus OnUpdate()
        {
            if (headPosition == null)
            {
                // Handle the case where headPosition is not set (e.g., log an error).
                return TaskStatus.Failure;
            }

            if(underGround.Value == true)
            {
                return TaskStatus.Failure;
            }
            MoveUnderground();
            timer -= Time.deltaTime;
            if(timer < 0)
            {
                timer = 1f;
                rtg.underGround = true;
                underGround.Value = true;
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Running;
            }
            /*
            Debug.DrawRay(headPosition.Value.position, Vector3.up * 10f, Color.red);
            if (IsUnderGround(headPosition.Value))
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Running;
            }
            */

            
            
        }

        // Implement the IsUnderGround method to check if the head position is under the ground.
        private bool IsUnderGround(Transform headTransform)
        {
            // Use raycasting to check if the head position is under the ground.
            RaycastHit hit;
            if (Physics.Raycast(headTransform.position, Vector3.up, out hit, maxDistanceToGround))
            {
                int targetLayer = LayerMask.NameToLayer("Ground");
                if(hit.collider.gameObject.layer == targetLayer)
                {
                    Debug.Log("hi");
                    finished = true;
                    return true;
                }
                
            }
            return false;
        }

        // Implement the MoveUnderground method to move the cactus underground.
        private void MoveUnderground()
        {
            // Translate the cactus downward to move it underground.
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        }
    }
}

