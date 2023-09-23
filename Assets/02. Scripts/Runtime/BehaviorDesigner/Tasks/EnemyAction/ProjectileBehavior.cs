using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public abstract class ProjectileBehavior : MonoBehaviour
    {
        public Vector3 targetPos { get; set; }
        public float travelTime { get; set; }

        public Transform target;
        public Vector3 destWithOffset;
        public string[] collisionTagsToCheck;
        public float duration, rotationSpeed, beforeTurnSpeed, afterTurnSpeed, defaultDestinationDistance, distanceBeforeTurn, destroyDelay;

        Vector3 startPosition, faceDirection, goingToPosition;
        float distance;

        Quaternion rotation;
        ParticleSystem loopFX, impactFX;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        protected abstract void Init();


        public void Explode()
        {
            Destroy(this.gameObject);
        }
        

        
    }
}

