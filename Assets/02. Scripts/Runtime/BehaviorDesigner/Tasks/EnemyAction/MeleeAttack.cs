using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class MeleeAttack : EnemyAction<Boss1Entity>
    {

        
        private Transform playerTrans;
        private bool collisionFlag;

        public float knockBackForce;

        public override void OnAwake() {
            base.OnAwake();
        }

        public override void OnStart()
        {
            base.OnStart();
            collisionFlag = false;
            playerTrans = GetPlayer().transform;
            anim.SetTrigger("Attack");
        }
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;

        }
        public override void OnCollisionEnter(Collision collision) {
            base.OnCollisionEnter(collision);
            // Debug.Log("Collision");
            if (GetComponent<Boss1>().slamHitBox.isActiveAndEnabled&&collision.collider.gameObject.CompareTag("Player")&& !collisionFlag) {
                Vector3 dir = playerTrans.position - transform.position;
                dir.y = 0;
                //make it 45 degrees from the ground
                dir = Quaternion.AngleAxis(45, Vector3.Cross(dir, Vector3.up)) * dir;
                dir.Normalize();
                collision.gameObject.GetComponent<Rigidbody>().AddForce(dir * knockBackForce, ForceMode.Impulse);
                collisionFlag = true;
                // Debug.Log("Hit player");
                
                
                
                /*HitBox.TriggerCheckHit(playerTrans.Value.GetComponentInChildren<HurtBox>(true)
                    .GetComponent<Collider>());*/
            }
        }

        public override void OnTriggerEnter(Collider collider)
        {
            base.OnTriggerEnter(collider);
           
        }
    }
}
