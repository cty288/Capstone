using MikroFramework.Architecture;
using Runtime.Player;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Base
{
    public abstract class AbstractProjectileWeaponViewController<T> : AbstractWeaponViewController<T>, IHitResponder
        where T : class, IWeaponEntity, new() {
        
        protected override IHitDetector OnCreateHitDetector() {
            return null;
        }
        
        public override bool CheckHit(HitData data)
        {
            return data.Hurtbox.Owner != gameObject;
        }
        
        public override void HitResponse(HitData data) {
            // TODO: Optimize projectile when we make one, or it might be using the old system for this.
            Instantiate(hitParticlePrefab, data.HitPoint, Quaternion.identity);
        }
    }
}