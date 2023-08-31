using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Utilities.AnimationEvents
{
    public class AnimationSMBManager : MonoBehaviour
    {
        [SerializeField] private bool m_debug = false;
        [SerializeField] private UnityEvent<string> m_event = new UnityEvent<string>();
        public UnityEvent<string> Event { get => m_event; }

        private void Awake()
        {
            m_event.AddListener(OnAnimationEvent);
        }

        /// <summary>
        /// Called when an event is triggered in the animation. eventName is set in the inspector.
        /// </summary>
        /// <param name="eventName"></param>
        private void OnAnimationEvent(string eventName)
        {
            if (m_debug)
                Debug.Log("Animation Event: " + eventName);
        }
    }
}
