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


public class BladeSentinalHoningBlade : AbstractBulletViewController
{
    private float bulletSpeed;
    public GameObject particlePrefab;
    private SafeGameObjectPool pool;
    private GameObject particleInstance;
    private GameObject childToRotate;
    private float rotationSpeed = 90f;
    private Rigidbody rb;
    private Transform playerTrans;
    private Transform bossTrans;

    private bool activated = false;
    private bool tracking;
    private Action onBulletRecycled;

    private float honingDuration;
    protected override void OnBulletReachesMaxRange()
    {
        
    }

    protected override void OnBulletRecycled()
    {
        StopAllCoroutines();
        activated = false;
        tracking = false;
        rb.velocity = Vector3.zero;
        transform.forward = Vector3.forward;
    }

    protected override void OnHitObject(Collider other) {
        if (other.gameObject.GetComponent<IDamageableViewController>() != null) {
            RecycleToCache();
        }
    }

    protected override void OnHitResponse(HitData data)
    {
       
    }
    public void SetData(float bulletSpeed, float honingDuration,Transform player,Transform boss,Action onBulletRecycled)
    {
        
        this.bulletSpeed = bulletSpeed;
        playerTrans = player;
        bossTrans = boss;
        this.onBulletRecycled += onBulletRecycled;
        this.honingDuration = honingDuration;
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
        if (activated)
        {
            if (tracking)
            {
                GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
                childToRotate.transform.Rotate(0, 360 * Time.deltaTime, 0);
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            }
            else
            {
                if(Vector3.Distance(this.gameObject.transform.position, playerTrans.position) < 1) {
                    RecycleToCache();
                }
                GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;
            }

        }
        else
        {

           
            //rb.velocity = Vector3.zero;
        }
    }
    private void FixedUpdate()
    {
        if (activated)
        {
            if (tracking)
            {
                Vector3 directionToPlayer = playerTrans.position - transform.position;

                // Create a rotation that points in the calculated direction
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                // Smoothly interpolate between the current rotation and the target rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
            }
        }
        else
        {
            transform.rotation = bossTrans.rotation;
        }
        
    }

    public void Activate()
    {
        if (!activated)
        {
            activated = true;
            StartCoroutine(Track());
        }
    }

    IEnumerator Track()
    {
        tracking = true;
        yield return new WaitForSeconds(honingDuration);
        tracking = false;
    }
    
    public override void OnRecycled() {
        base.OnRecycled();
        onBulletRecycled?.Invoke();
        onBulletRecycled = null;
    }


}
