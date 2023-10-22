using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Utilities.AnimationEvents
{
    public enum AnimationSMB_Timing
    {
        OnEnter,
        OnUpdate,
        OnExit,
        OnEnd
    }

    /// <summary>
    /// AnimationSMB_Event can be placed on an animation state in the animator to
    /// invoke an event at a specific time in an animation clip.
    /// </summary>
    public class AnimationSMB_Event : StateMachineBehaviour
    {
        /// <summary>
        /// This is the data that will be used to invoke an event. Is visible in the inspector.
        /// </summary>
        [System.Serializable]
        public class AnimationSMB_EventData
        {
            public bool activated;
            public string eventName;
            public AnimationSMB_Timing timing;
            public float onUpdateFrame;
        }

        [SerializeField] private int totalFrames; //frames in animation clip
        [SerializeField] private int currentFrame; //current frame in animation
        [SerializeField] private float normalizedTime; //progress of animation clip between 0-1
        [SerializeField] private float normalizedTimeUncapped; //normalized time but goes beyond if animation is loop
        [SerializeField] private string motionTime = "";

        public List<AnimationSMB_EventData> events = new List<AnimationSMB_EventData>();
        private bool hasMotionTimeParam; //check if has motionTime parameter
        private AnimationSMBManager animationSMBManager; //reference to manager

        /// <summary>
        /// Called when the animation state is starts, frame=0.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            hasMotionTimeParam = HasParameter(animator, motionTime);
            animationSMBManager = animator.GetComponentInParent<AnimationSMBManager>();
            totalFrames = GetTotalFrames(animator, layerIndex);
            normalizedTimeUncapped = stateInfo.normalizedTime;
            normalizedTime = hasMotionTimeParam ? animator.GetFloat(motionTime) : GetNormalizedTime(stateInfo);
            currentFrame = GetFrame(normalizedTime, totalFrames);

            if (animationSMBManager != null)
            {
                foreach (AnimationSMB_EventData data in events)
                {
                    data.activated = false;
                    if (data.timing == AnimationSMB_Timing.OnEnter)
                    {
                        data.activated = true;
                        animationSMBManager.Event.Invoke(data.eventName);
                    }
                }
            }
        }

        /// <summary>
        /// Called on specific frames of the animation. Depends on the frame set in the inspector.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            normalizedTimeUncapped = stateInfo.normalizedTime;
            normalizedTime = hasMotionTimeParam ? animator.GetFloat(motionTime) : GetNormalizedTime(stateInfo);
            currentFrame = GetFrame(normalizedTime, totalFrames);

            if (animationSMBManager != null)
            {
                foreach (AnimationSMB_EventData data in events)
                {
                    if (!data.activated)
                    {
                        if (data.timing == AnimationSMB_Timing.OnUpdate)
                        {
                            if (currentFrame >= data.onUpdateFrame)
                            {
                                data.activated = true;
                                animationSMBManager.Event.Invoke(data.eventName);
                            }
                        }
                        else if (data.timing == AnimationSMB_Timing.OnEnd)
                        {
                            if (currentFrame >= totalFrames)
                            {
                                data.activated = true;
                                animationSMBManager.Event.Invoke(data.eventName);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when the animation state ends, frame=totalFrames.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animationSMBManager != null)
            {
                foreach (AnimationSMB_EventData data in events)
                {
                    if (data.timing == AnimationSMB_Timing.OnExit)
                    {
                        data.activated = true;
                        animationSMBManager.Event.Invoke(data.eventName);
                    }
                }
            }
        }

        private bool HasParameter(Animator animator, string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName) || string.IsNullOrWhiteSpace(parameterName))
                return false;

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == motionTime)
                    return true;
            }
            return false;
        }

        private float GetNormalizedTime(AnimatorStateInfo stateInfo)
        {
            return stateInfo.normalizedTime > 1 ? 1 : stateInfo.normalizedTime;
        }

        private int GetTotalFrames(Animator animator, int layerIndex)
        {
            AnimatorClipInfo[] clipInfo = animator.GetNextAnimatorClipInfo(layerIndex);
            if (clipInfo.Length == 0)
            {
                clipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex);
            }

            AnimationClip clip = clipInfo[0].clip;
            return Mathf.RoundToInt(clip.length * clip.frameRate);
        }

        private int GetFrame(float normalizedTime, int totalFrames)
        {
            return Mathf.RoundToInt(normalizedTime * totalFrames);
        }
    }
}