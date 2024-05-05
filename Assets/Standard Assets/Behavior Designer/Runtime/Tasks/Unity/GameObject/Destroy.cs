using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject
{
    [TaskCategory("Unity/GameObject")]
    [TaskDescription("Destorys the specified GameObject. Returns Success.")]
    public class Destroy : Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;
        public bool destroyThis;
        [Tooltip("Time to destroy the GameObject in")]
        public float time;
        

        public override TaskStatus OnUpdate()
        {
            if(destroyThis)
            {
                GameObject.Destroy(this.gameObject);
                return TaskStatus.Success;
            }
            var destroyGameObject = GetDefaultGameObject(targetGameObject.Value);
            if (time == 0) {
                GameObject.Destroy(destroyGameObject);
            } else {
                GameObject.Destroy(destroyGameObject, time);
            }

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetGameObject = null;
            time = 0;
        }
    }
}