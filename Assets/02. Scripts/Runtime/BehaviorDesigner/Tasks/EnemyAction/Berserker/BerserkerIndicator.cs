using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using UnityEngine;

public class BerserkerIndicator : AbstractBulletViewController
{
    protected override void OnBulletReachesMaxRange()
    {
        //throw new System.NotImplementedException();
    }

    protected override void OnBulletRecycled()
    {
        //throw new System.NotImplementedException();
    }

    protected override void OnHitObject(Collider other)
    {
       // throw new System.NotImplementedException();
    }

    protected override void OnHitResponse(HitData data)
    {
        //throw new System.NotImplementedException();
    }
}
