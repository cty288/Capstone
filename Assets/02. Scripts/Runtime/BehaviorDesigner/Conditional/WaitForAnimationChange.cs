using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using UnityEngine;

namespace Runtime.BehaviorDesigner.Conditional
{
    public class WaitForAnimationChange : IsName
    {
        private Animator animator;

        private GameObject prevGameObject;
        
        private bool isWaiting = false;
        [SerializeField] private float maxWaitTimeBeforeEnt = 2f;
        private float currentWaitTime = 0f;

        public override void OnStart()
        {
            var currentGameObject = GetDefaultGameObject(targetGameObject.Value);
            if (currentGameObject != prevGameObject)
            {
                animator = currentGameObject.GetComponent<Animator>();
                prevGameObject = currentGameObject;
            }
            currentWaitTime = 0f;
        }

        public override TaskStatus OnUpdate()
        {
            if (animator == null)
            {
                Debug.LogWarning("Animator is null");
                return TaskStatus.Failure;
            }

            if (!isWaiting) {
                currentWaitTime += Time.deltaTime;
                if (currentWaitTime >= maxWaitTimeBeforeEnt)
                {
                    return TaskStatus.Failure;
                }
            }
            
            if (Animator.StringToHash(name.Value) ==
                animator.GetCurrentAnimatorStateInfo(index.Value).shortNameHash && !isWaiting)
            {
                isWaiting = true;
            }
            else if (isWaiting && Animator.StringToHash(name.Value) !=
                     animator.GetCurrentAnimatorStateInfo(index.Value).shortNameHash)
            {
                return TaskStatus.Success;
            }
            
            return TaskStatus.Running;
        }
    }
}