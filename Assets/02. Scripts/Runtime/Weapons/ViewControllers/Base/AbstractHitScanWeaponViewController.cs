using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using Runtime.Player;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;
using UnityEngine.VFX;

namespace Runtime.Weapons.ViewControllers.Base
{
    public abstract class AbstractHitScanWeaponViewController<T> : AbstractWeaponViewController<T>, IHitResponder
        where T : class, IWeaponEntity, new() {
        
        [Header("Aesthetic")]
        public VisualEffect[] bulletVFX;
        protected HitDetectorInfo hitDetectorInfo;
        
        protected override void OnEntityStart()
        {
            base.OnEntityStart();
            
            hitDetector = OnCreateHitDetector();
            hitDetectorInfo = new HitDetectorInfo
            {
                camera = cam,
                layer = layer,
                launchPoint = bulletVFX[0].transform,
                weapon = BoundEntity
            };
            
            // TODO: Phase out old Particle System
            if (hitVFXSystem)
            {
                isHitVFX = true;
            }
        }
        
        protected abstract IHitDetector OnCreateHitDetector();

        protected override void Shoot()
        {
            base.Shoot();
            crossHairViewController?.OnShoot();
            BoundEntity.OnRecoil(IsScopedIn);
            hitDetector.CheckHit(hitDetectorInfo, BoundEntity.GetRealDamageValue());
        }
        
        public override void HitResponse(HitData data) {
            Debug.Log("AbstractHitScanWeaponViewController HitResponse");
            // TODO: Phase out old Particle System
            hitVFXSystem.SetVector3("StartPosition", data.HitPoint);
            hitVFXSystem.SetVector3("HitNormal", data.HitNormal);
            hitVFXSystem.Play();
        }
    }
}