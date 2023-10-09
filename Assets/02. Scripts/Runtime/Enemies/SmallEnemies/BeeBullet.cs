using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;

public class BeeBullet : AbstractBulletViewController
{
    protected override void OnHitObject(Collider other) {
        
    }

    protected override void OnBulletRecycled()
    {
        
    }

    protected override void OnHitResponse(HitData data)
    {
        
    }
    private void Update()
    {
        Debug.Log("shooting");
    }

    protected override void OnBulletReachesMaxRange() {
        
    }
}
