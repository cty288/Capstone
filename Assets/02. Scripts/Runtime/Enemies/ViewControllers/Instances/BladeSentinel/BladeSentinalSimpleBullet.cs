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

    protected override void OnBulletReachesMaxRange()
    {
        throw new NotImplementedException();
    }

    protected override void OnBulletRecycled()
    {
        throw new NotImplementedException();
    }

    protected override void OnHitObject(Collider other)
    {
        throw new NotImplementedException();
    }

    protected override void OnHitResponse(HitData data)
    {
        throw new NotImplementedException();
    }
    public void SetData(float speed, Vector3 dir)
    {
        this.speed = speed;
        this.dir = dir;
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
        rb.AddForce(this.gameObject.transform.forward * 8f);
    }
}
