using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using Runtime.Enemies;
using Runtime.Utilities.Collision;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class Roll : EnemyAction
    {
        public SharedTransform playerTrans;
        Rigidbody rb;
        private Vector3 targetLocation;
        //public int dashVelocity;
        //public SharedFloat dashTime;
        public float abortDistance = 20f;
        public float forceToPlayer = 50f;
        private NavMeshAgent navMeshAgent;
        private float ogSpeed;
        private int currentCorner = 0;
        
        // private NavMeshPath path = new NavMeshPath();
        private Vector3[] corners;
        private float timeStart;
        private Rigidbody playerRb;

        //Joon's implementation variables
        // public GameObject shell;
        private bool chargeUp = true;
        // public float movementSpeed;
        private Vector3 localSavePlayerPosition;
        private bool hasMovedPassPlayer;
        private Vector3 dir;
        private float timer = 3f;
        public float maxRotationSpeed = 260; // Maximum rotation speed (degrees per second)
        
        private float currentRotationSpeed = 0.0f;
        public GameObject pivot;
        public bool stun;
        private SharedVector3 initRotation;
        private float chargeTime = 3.0f;  // Total charge time in seconds
        private float currentChargeTime = 0.0f;
        private float initialRotationSpeed = 0.0f;
        private float rotationDecreaseRate = 160f;
        private bool flag = false;
        private bool collisionFlag = false;

        private Collider bossCollider;
        
        public HitBox HitBox;

        public override void OnStart()
        {
            //reset
            flag = false;
            maxRotationSpeed = 260;
            initialRotationSpeed = 0f;
            initRotation = this.gameObject.transform.eulerAngles;
            currentChargeTime = 0f;
            rb = GetComponent<Rigidbody>();
            playerRb = playerTrans.Value.GetComponent<Rigidbody>();
            bossCollider = GetComponent<Collider>();
            /*
            targetLocation = playerTrans.Value.position + transform.forward * 3;
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
            ogSpeed = navMeshAgent.speed;
            navMeshAgent.CalculatePath(targetLocation, path);
            corners = path.corners;
            */


            HitBox.GetComponentInParent<Boss1>().ClearHitObjects();
            HitBox.StartCheckingHits();
            collisionFlag = false;
            rb.isKinematic = false;
            chargeUp = true;
            timer = 3f;
            bossCollider.enabled = true;
            
        }
        // public override TaskStatus OnUpdate()
        // {
        //     if (Time.time >= timeStart + dashTime.Value){
        //         // Debug.Log("Done");
        //         rb.velocity = Vector3.zero;
        //         navMeshAgent.speed = ogSpeed;
        //         return TaskStatus.Success;
        //     }
        //     else
        //     {
        //         Debug.Log("moving");
        //         if (corners != null && currentCorner < corners.Length)
        //         {
        //             Vector3 direction = corners[currentCorner] - transform.position;
        //             if (direction.magnitude < 0.5f)
        //             {
        //                 currentCorner++;
        //             }
        //             else
        //             {
        //                 rb.AddForce(direction.normalized * dashVelocity * 100 * Time.deltaTime);
        //             }
        //         }
        //         return TaskStatus.Running;
        //     }
        // }
        
        public override TaskStatus OnUpdate()
        {
           
            if (stun)
            {
                Debug.Log("stunned");
                    
                pivot.transform.DORotate((Vector3)initRotation.Value, 5f).OnComplete(() =>
                {
                    NavMeshAgent navMeshComponent = GetComponent<NavMeshAgent>();
                    if (navMeshComponent != null)
                    {
                        navMeshComponent.enabled = true;
                    }
                });
                return TaskStatus.Running;
            }
            if (chargeUp)
            {
                Debug.Log("chargning");
                if (currentChargeTime < chargeTime)
                {
                    // Gradually increase the rotation speed over the charge time
                    initialRotationSpeed += (maxRotationSpeed / chargeTime) * Time.deltaTime;
                    currentChargeTime += Time.deltaTime;

                    // Rotate the object based on the current rotation speed
                    pivot.transform.Rotate(Vector3.forward * initialRotationSpeed * Time.deltaTime);
                }
                else
                {
                    localSavePlayerPosition = playerTrans.Value.position;
                    dir = (localSavePlayerPosition - this.transform.position).normalized;
                    chargeUp = false;
                }

                return TaskStatus.Running;
            }
            else
            {
               

                if(flag)
                {
                    
                   /*
                   // dir = this.transform.forward;
                    //Vector3 dragForce = -rb.velocity * 0.005f;
                    //rb.AddForce(dragForce, ForceMode.Impulse);
                    
                        
                    maxRotationSpeed -= rotationDecreaseRate * Time.deltaTime;
                    pivot.transform.Rotate(new Vector3(1, 1, 1) * maxRotationSpeed * Time.deltaTime);
                    if(maxRotationSpeed < 0)
                    {
                        chargeUp = true;
                        return TaskStatus.Success;*/
                    //}
                    
                    //lerp velocity to 0
                    
                 
                    if(rb.velocity.magnitude < 0.5f) {
                        rb.velocity = Vector3.zero;
                        return TaskStatus.Success;
                    }
                    else {
                        rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 1f * Time.deltaTime);
                        Debug.Log("slowing down");
                        return TaskStatus.Running;
                    }
                }
                else
                {
                    if(Vector3.Distance(localSavePlayerPosition, this.transform.position) < abortDistance || timer < 0) {
                        flag = true;
                        return TaskStatus.Running;
                    }
                    maxRotationSpeed = 260;
                    pivot.transform.Rotate(new Vector3(1,1,1)* maxRotationSpeed * Time.deltaTime);
                    dir = (localSavePlayerPosition - this.transform.position).normalized;
                    Debug.DrawRay(this.transform.position, dir * 100, Color.green);
                    //CheckForCollisions();
                    rb.AddForce(dir * 0.25f, ForceMode.Impulse);
                    Debug.Log(Vector3.Distance(localSavePlayerPosition, this.transform.position));
                    //this.gameObject.transform.Translate(dir * 20 * Time.deltaTime , Space.World);
                    timer -= Time.deltaTime;
                    return TaskStatus.Running;
                }
               
            }
            
            /*
            if (corners != null && currentCorner < corners.Length)
            {
                Vector3 direction = corners[currentCorner] - transform.position;
                if (direction.magnitude < 0.5f)
                {
                    currentCorner++;
                    return TaskStatus.Running;
                }
                else
                {
                    rb.AddForce(direction.normalized * dashVelocity * 20 * Time.deltaTime);
                    return TaskStatus.Running;
                }
            }
            return TaskStatus.Success;
            */
        }


        public override void OnEnd() {
            base.OnEnd();
            rb.isKinematic = true;
            pivot.transform.DORotate((Vector3)initRotation.Value, 2f).OnComplete(() =>
            {
                NavMeshAgent navMeshComponent = GetComponent<NavMeshAgent>();
                if (navMeshComponent != null) {
                    navMeshComponent.enabled = true;
                }
                HitBox.StopCheckingHits();
            });
            
            bossCollider.enabled = false;
        }

        public override void OnCollisionEnter(Collision collision) {
            base.OnCollisionEnter(collision);
            Debug.Log("Collision");
            if (collision.collider.gameObject.CompareTag("Player") && !collisionFlag) {
                Vector3 dir = playerTrans.Value.position - transform.position;
                dir.y = 0;
                //make it 45 degrees from the ground
                dir = Quaternion.AngleAxis(20, Vector3.Cross(dir, Vector3.up)) * dir;
                dir.Normalize();
                playerRb.AddForce(dir * forceToPlayer, ForceMode.Impulse);
                flag = true;
                collisionFlag = true;
                Debug.Log("Hit player");
                
                
                
                /*HitBox.TriggerCheckHit(playerTrans.Value.GetComponentInChildren<HurtBox>(true)
                    .GetComponent<Collider>());*/
            }
        }
    }
}
