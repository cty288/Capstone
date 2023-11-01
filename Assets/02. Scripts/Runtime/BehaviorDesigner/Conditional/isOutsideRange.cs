using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using UnityEngine;

namespace Runtime.BehaviorDesigner.Conditional
{
    public class IsOutsideRange : EnemyConditional
    {
        public SharedGameObject target;
        public float treshold;

        public override void OnStart()
        {
            
        }

        public override TaskStatus OnUpdate()
        {
            if(Vector3.Distance(target.Value.transform.position,this.gameObject.transform.position) > treshold)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
    }
}