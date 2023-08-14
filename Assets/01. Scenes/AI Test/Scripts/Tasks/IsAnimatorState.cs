using UnityEngine;

/*
Modified from Animator IsName Conditional Task.
*/
namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator
{
    [TaskCategory("Unity/Animator")]
    [TaskDescription("Returns success if the specified short animation state name matches the name of the active state of an Animator.")]
    public class IsAnimatorState : IsName
    {
        private Animator animator;
        private GameObject prevGameObject;

        public override void OnStart()
        {
            var currentGameObject = GetDefaultGameObject(targetGameObject.Value);
            if (currentGameObject != prevGameObject)
            {
                animator = currentGameObject.GetComponent<Animator>();
                prevGameObject = currentGameObject;
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (animator == null)
            {
                Debug.LogWarning("Animator is null");
                return TaskStatus.Failure;
            }

            return Animator.StringToHash(name.Value) == animator.GetCurrentAnimatorStateInfo(index.Value).shortNameHash ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            base.OnReset();
        }
    }
}