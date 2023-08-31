using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class EnemyAction : Action
    {
        protected Rigidbody body;
        protected Animator anim;
    

        public override void OnAwake()
        {
            body = GetComponent<Rigidbody>();
            anim = gameObject.GetComponent<Animator>();
        
        }

    }
}
