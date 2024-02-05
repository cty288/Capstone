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

public class BladeSentinalSimpleBullet : AbstractBulletViewController
{
    float speed;
    Rigidbody rb;
    Vector3 dir;
    bool start;

    protected override void OnBulletReachesMaxRange()
    {
     
    }

    protected override void OnBulletRecycled()
    {
       
    }

    protected override void OnHitObject(Collider other)
    {
       
    }

    protected override void OnHitResponse(HitData data)
    {
      
    }
    public void SetData(float speed, Vector3 dir , bool start)
    {
        this.speed = speed;
        this.dir = dir;
        this.start = start;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        this.gameObject.transform.LookAt(dir);

    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {

            rb.AddForce(this.gameObject.transform.forward * 8f);
        }
    }
}
