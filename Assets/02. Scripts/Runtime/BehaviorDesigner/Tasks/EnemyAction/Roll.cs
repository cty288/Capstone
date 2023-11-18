using System.Collections.Generic;
using System.Security.Policy;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine.Animations.Rigging;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class Roll : EnemyAction<Boss1Entity>
    {
        Rigidbody rb;
        private Vector3 targetLocation;
        //public int dashVelocity;
        //public SharedFloat dashTime;
        public float abortDistance = 20f;
        public float forceToPlayer = 50f;
        private NavMeshAgent navMeshAgent;
        private float ogSpeed;
        private int currentCorner = 0;
        
        private NavMeshPath path = new NavMeshPath();
        private Vector3[] corners;
        private float timeStart;
        private Rigidbody playerRb;

        //Joon's implementation variables
        public GameObject shell;
        private bool chargeUp = true;
        public float movementSpeed;
        private Vector3 localSavePlayerPosition;
        private bool hasMovedPassPlayer;
        private Vector3 dir;
        private float timer = 3f;
        public float maxRotationSpeed = 260; // Maximum rotation speed (degrees per second)
        
        private float currentRotationSpeed = 0.0f;
        public GameObject pivot;
       
        private SharedVector3 initRotation;
        private float chargeTime = 2.0f;  // Total charge time in seconds
        private float currentChargeTime = 0.0f;
        private float initialRotationSpeed = 0.0f;
        private float rotationDecreaseRate = 160f;
        private bool flag = false;
        //private bool collisionFlag = false;
        private HashSet<Rigidbody> hitObjects = new HashSet<Rigidbody>();

        private Collider bossCollider;
        private bool canDealDamage = false;
        public HitBox HitBox;
        
        private Transform playerTrans;

        public override void OnStart()
        {
            base.OnStart();
            //reset
            flag = false;
            maxRotationSpeed = 260;
            initialRotationSpeed = 0f;
            initRotation = this.gameObject.transform.eulerAngles;
            currentChargeTime = 0f;
            rb = GetComponent<Rigidbody>();
            playerTrans = GetPlayer().transform;
            playerRb = playerTrans.GetComponent<Rigidbody>();
            bossCollider = GetComponent<Collider>();
         

            HitBox.GetComponentInParent<Boss1>().ClearHitObjects();
            
            hitObjects.Clear();
            //collisionFlag = false;
            rb.isKinematic = false;
            chargeUp = true;
            timer = 3f;
            bossCollider.enabled = true;
            
        }
       
        public override TaskStatus OnUpdate()
        {
           
           
            if (chargeUp)
            {
                // Debug.Log("chargning");
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
                    localSavePlayerPosition = playerTrans.position;
                    dir = (localSavePlayerPosition - this.transform.position).normalized;
                    chargeUp = false;
                    canDealDamage = true;
                    HitBox.StartCheckingHits(enemyEntity.GetCustomDataValue<int>("damages", "rollDamage"));
                }

                return TaskStatus.Running;
            }
            else
            {
                if(flag)
                {
               
                    
                    if (rb.velocity.magnitude <= 2f && canDealDamage) {
                        HitBox.StopCheckingHits();
                        canDealDamage = false;
                    }
                 
                    if(rb.velocity.magnitude < 0.5f) {
                        rb.velocity = Vector3.zero;
                        return TaskStatus.Success;
                    }
                    else {
                        rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 1f * Time.deltaTime);
                        // Debug.Log("slowing down");
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
                    // Debug.DrawRay(this.transform.position, dir * 100, Color.green);
                    //CheckForCollisions();
                    rb.AddForce(dir * 250f, ForceMode.Acceleration);
                    // Debug.Log(Vector3.Distance(localSavePlayerPosition, this.transform.position));
                    //this.gameObject.transform.Translate(dir * 20 * Time.deltaTime , Space.World);
                    timer -= Time.deltaTime;
                    return TaskStatus.Running;
                }
               
            }
            
          
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
            
            //bossCollider.enabled = false;
            canDealDamage = false;
        }

        public override void OnCollisionEnter(Collision collision) {
            base.OnCollisionEnter(collision);
            // Debug.Log("Collision");
            Rigidbody attachedRigidbody = collision.collider.attachedRigidbody;
            
            if (canDealDamage && attachedRigidbody && attachedRigidbody.GetComponent<ICreatureViewController>() != null
                && !hitObjects.Contains(collision.collider.attachedRigidbody)) {
                Vector3 dir = playerTrans.position - transform.position;
                dir.y = 0;
                //make it 45 degrees from the ground
                dir = Quaternion.AngleAxis(45, Vector3.Cross(dir, Vector3.up)) * dir;
                dir.Normalize();
                attachedRigidbody.AddForce(dir * forceToPlayer, ForceMode.Impulse);
                flag = true;
                hitObjects.Add(attachedRigidbody);
            }
        }
    }
}
