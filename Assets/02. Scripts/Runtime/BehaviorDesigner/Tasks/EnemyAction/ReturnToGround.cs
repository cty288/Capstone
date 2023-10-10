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
        public BehaviorTree tree;
        public SharedVector3 returnPosition;
        public SharedVector3 initialPosition;
        public override void OnStart()
        {
            tree = this.gameObject.GetComponent<BehaviorTree>();
            initial = true;
            SharedBool isInit = (SharedBool)tree.GetVariable("Init");
            isInit.SetValue(true);
            SharedVector3 pos = (SharedVector3)tree.GetVariable("InitialPosition");
            returnPosition = pos;



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
                if((this.gameObject.transform.position.y <= returnPosition.Value.y))
                {
                    MoveUp();
                }
                
                Debug.DrawRay(headPosition.Value.position, Vector3.up * 10f, Color.red);
                timer -= Time.deltaTime;
                Debug.Log("asdf");
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
        public override void OnEnd()
        {
            base.OnEnd();
            foreach (var d in dug)
            {
                d.underGround = false;
            }

        }

        // Implement the MoveUnderground method to move the cactus underground.
        private void MoveUp()
        {
          
            
                this.gameObject.transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
            
        }
    }
}

