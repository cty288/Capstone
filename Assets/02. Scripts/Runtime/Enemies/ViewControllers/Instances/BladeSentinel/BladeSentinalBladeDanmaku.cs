using System;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using UnityEngine;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using System.Collections;
using System.Collections.Generic;
using Runtime.DataFramework.ViewControllers.Entities;


public class BladeSentinalBladeDanmaku : AbstractBulletViewController
{
    private float bulletSpeed;
    public GameObject particlePrefab;
    private SafeGameObjectPool pool;
    private GameObject particleInstance;
    private GameObject childToRotate;
    private float rotationSpeed = 90f;
    private float afterBulletSpeed;
    private float afterRotationSpeed;
    private bool secondPhase = false;
    private Rigidbody rb;
    private Transform rotateAroundPoint;
    private bool thirdPhase = false;
    private Transform playerTrans;
    private bool canAttack = false;
    private Vector3 playerTarget;
    private Action onBulletRecycled;
    private int attackVersion;
    protected override void OnBulletReachesMaxRange()
    {
        
    }

    protected override void OnBulletRecycled()
    {
        thirdPhase = false;
        secondPhase = false;
        canAttack = false;
        transform.rotation = Quaternion.identity;
        rotateAroundPoint = null;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    protected override void OnHitObject(Collider other) {
        if (other.gameObject.GetComponent<IDamageableViewController>() != null) {
            RecycleToCache();
        }
    }

    protected override void OnHitResponse(HitData data)
    {
       
    }
    public void SetData(float bulletSpeed , float initTime, float afterBulletSpeed , float afterRotationSpeed , Transform origin , Transform player,
        Action onBulletRecycled , int attackVersion)
    {
     
        
        this.bulletSpeed = bulletSpeed;
        this.afterRotationSpeed = afterRotationSpeed;
        this.afterBulletSpeed = afterBulletSpeed;
        if(attackVersion == 0)
        {
            Invoke("SetSpeed", initTime);
            Invoke("SetRotationSpeed", initTime);
        }
        if(attackVersion == 1)
        {

            Invoke("SetSpeed", initTime);
            Invoke("SetRotationSpeed", initTime);
        }
        //Invoke("SetPhase", 6f);
        //Invoke("FinalPhase", 10f);
        rotateAroundPoint = origin;
        playerTrans = player;
        this.onBulletRecycled += onBulletRecycled;
        this.attackVersion = attackVersion;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        childToRotate = this.gameObject.transform.GetChild(0).gameObject;
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!secondPhase && !thirdPhase && attackVersion == 1)
        {

            this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
            childToRotate.transform.Rotate(0, 360 * Time.deltaTime, 0);
            this.gameObject.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
        else if(!secondPhase && !thirdPhase && attackVersion == 0)
        {
            this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
            childToRotate.transform.Rotate(0, 360 * Time.deltaTime, 0);
            this.gameObject.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
        else
        {

           
            //rb.velocity = Vector3.zero;
        }
    }
    private void FixedUpdate()
    {
        if (!rotateAroundPoint) {
            return;
        }
        if (secondPhase)
        {
            this.gameObject.transform.localScale = new Vector3(10, 10, 10);
            this.gameObject.transform.RotateAround(rotateAroundPoint.position, Vector3.up, 20 * Time.deltaTime);


            if(rb.velocity.magnitude < 0.05f)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                float dampingFactor = 0.98f; // You can adjust this value based on your preference

                // Apply a force in the opposite direction of the current velocity
                rb.AddForce(-rb.velocity * dampingFactor, ForceMode.Acceleration);
            }
            
            Quaternion deltaQuat = Quaternion.FromToRotation(this.gameObject.transform.forward, Vector3.up);

            Vector3 axis;
            float angle;
            deltaQuat.ToAngleAxis(out angle, out axis);

            float dampenFactor = 0.8f; // this value requires tuning
            rb.AddTorque(-rb.angularVelocity * dampenFactor, ForceMode.Acceleration);

            float adjustFactor = 0.5f; // this value requires tuning
            rb.AddTorque(axis.normalized * angle * adjustFactor, ForceMode.Acceleration);

            
        }
        if (thirdPhase)
        {
            if (canAttack)
            {
                Vector3 directionToPlayer = playerTrans.position - transform.position;
                this.gameObject.GetComponent<Rigidbody>().velocity = directionToPlayer * bulletSpeed;
                if(Vector3.Distance(this.gameObject.transform.position, playerTrans.position) < 1) {
                    RecycleToCache();
                }

            }
            else
            {

                secondPhase = false;
                Vector3 directionToPlayer = playerTrans.position - transform.position;

                // Create a rotation that points in the calculated direction
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                // Smoothly interpolate between the current rotation and the target rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.deltaTime);

                if (Quaternion.Angle(transform.rotation, targetRotation) < 5f)
                {
                    canAttack = true;
                }
            }
        }
    }

    public override void OnRecycled() {
        base.OnRecycled();
        onBulletRecycled?.Invoke();
        onBulletRecycled = null;
    }

    private void SetSpeed()
    {
        this.bulletSpeed = afterBulletSpeed;
    }
    private void SetRotationSpeed()
    {
        this.rotationSpeed = afterRotationSpeed;
    }
    private void SetPhase()
    {
        secondPhase = true;
    }
    private void FinalPhase()
    {
        playerTarget = playerTrans.position;
        thirdPhase = true;
    }
    private void GetPlayerTransform()
    {
        playerTarget = playerTrans.position + new Vector3(0,2,0);
    }

}
