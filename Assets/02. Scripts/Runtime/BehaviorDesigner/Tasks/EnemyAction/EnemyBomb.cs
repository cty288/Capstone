using UnityEngine;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class EnemyBomb : ProjectileBehavior
    {
        
       
        private float startTime;
        private bool slerping;
        public bool test;
        //local add-on
        Vector3 startPosition, faceDirection, goingToPosition;
        float distance;
        bool hasTurned = false, isDead = false;
        Quaternion rotation;
        ParticleSystem loopFX, impactFX;

        // Start is called before the first frame update
        protected override void Start()
        {
            startPosition = transform.position;
            var rot = transform.rotation.eulerAngles;
            //rot.y = Random.Range(-90, 90);
            //rot.x = Random.Range(-45, 45);
            //rot.z = Random.Range(-30, 30);
            transform.rotation = Quaternion.Euler(rot);
            
           
        }

        // Update is called once per frame
        void Update()
        {
            if (isDead) { return; }


            if (slerping)
            {
                float fracComplete = (Time.time - startTime) / travelTime;
                transform.position = Vector3.Slerp(transform.position, targetPos, fracComplete);
                if (Vector3.Distance(transform.position,targetPos) <= 1)
                {
                    slerping = false;
                }
            }

            if (hasTurned)
            {
                if (destWithOffset != null)
                {
                    faceDirection = destWithOffset - transform.position;
                    goingToPosition = destWithOffset;
                }

                rotation = Quaternion.LookRotation(faceDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);

                transform.Translate(transform.forward * afterTurnSpeed * Time.deltaTime, Space.World);

                distance = Vector3.Distance(transform.position, goingToPosition);
                if (distance <= 0.4f)
                {
                    Explode();
                }
            }
            else
            {
                transform.Translate(transform.up * beforeTurnSpeed * Time.deltaTime, Space.World);
                //transform.Translate(Vector3.up * beforeTurnSpeed * Time.deltaTime, Space.World);
                distance = Vector3.Distance(transform.position, startPosition);
                if (distance > distanceBeforeTurn)
                {
                    hasTurned = true;
                }
            }

            if (duration <= 0)
            {
                Explode();
            }
            else
            {
                duration -= Time.deltaTime;
            }




        }
        public void Init(Transform target,float tTime)
        {
            slerping = true;
            targetPos = target.position;
            startTime = Time.time;
            travelTime = tTime;
        }

        protected override void Init()
        {
            
        }
    }
}
