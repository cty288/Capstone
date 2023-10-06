using System.Collections;
using System.Collections.Generic;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Enemies
{
    public class Example_Explosion : AbstractExplosionViewController
    {


        protected override void OnHitResponse(HitData data)
        {
            
        }

        protected override void OnBulletRecycled()
        {
            StopAllCoroutines();
        }
    }

}
