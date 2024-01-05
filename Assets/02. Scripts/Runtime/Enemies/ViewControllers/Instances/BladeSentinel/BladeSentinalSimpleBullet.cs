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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
