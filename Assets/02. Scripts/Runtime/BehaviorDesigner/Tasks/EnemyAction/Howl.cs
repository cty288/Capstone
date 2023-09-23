using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class Howl : EnemyAction
    {
        public Animator tigerAnimator; // Reference to the tiger's Animator component.
        public string howlAnimationName = "Howl"; // The name of the howl animation.
        public SharedBool howling; // A shared bool to keep track of whether the tiger is howling.

        private bool animationStarted = false;

        public override void OnAwake()
        {
            // Ensure the tiger's Animator component is valid.
            if (tigerAnimator == null)
            {
                Debug.LogError("Tiger Animator is not assigned.");
                return;
            }
        }

        public override TaskStatus OnUpdate()
        {
            // Play the howl animation if it hasn't started already.
            if (!animationStarted)
            {
                tigerAnimator.Play(howlAnimationName);
                animationStarted = true;
                howling.Value = true; // Set the shared bool to true while howling.
            }

            // Check if the howl animation is still playing.
            if (!tigerAnimator.GetCurrentAnimatorStateInfo(0).IsName(howlAnimationName))
            {
                animationStarted = false;
                howling.Value = false; // Set the shared bool to false when howling is done.
                return TaskStatus.Success; // Animation is done, so move to the next sequence.
            }

            return TaskStatus.Running; // Continue playing the howl animation.
        }
    }
}
