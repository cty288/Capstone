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
    public class ReturnToGround : EnemyAction
    {
        public SharedTransform headPosition;
        public SharedBool initial = false;
        public SharedBool underGround;
        public float moveSpeed;
        public float timer = 1f;
        public DiveUnderGround[] dug = new DiveUnderGround[2];
        public override void OnStart()
        {
            initial = true;
            
        }

        public override TaskStatus OnUpdate()
        {
            if (headPosition == null)
            {
                // Handle the case where headPosition is not set (e.g., log an error).
                return TaskStatus.Failure;
            }
            if (underGround.Value == true)
            {
                MoveUp();
                Debug.DrawRay(headPosition.Value.position, Vector3.up * 10f, Color.red);
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    underGround.Value = false;
                    foreach (var d in dug)
                    {
                        d.underGround = false;
                    }
                    timer = 1f;
                    return TaskStatus.Success;
                }
                else
                {
                    return TaskStatus.Running;
                }
            }
            else
            {
                return TaskStatus.Success;
            }

           


        }

        // Implement the MoveUnderground method to move the cactus underground.
        private void MoveUp()
        {
            // Translate the cactus downward to move it underground.
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        }
    }
}

