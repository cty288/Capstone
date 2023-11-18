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
        public TrailRenderer trailRenderer;
        public VisualEffect[] bulletVFX;
        
        private HitDetectorInfo hitDetectorInfo;
        
        
        protected override void OnEntityStart()
        {
            base.OnEntityStart();
            
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
        
        protected override IHitDetector OnCreateHitDetector() {
            return new HitScan(this, CurrentFaction.Value, bulletVFX, fpsCamera);
        }
        
        public virtual void SetShoot(bool shouldShoot) {
            if (shouldShoot) {
                crossHairViewController?.OnShoot();
                BoundEntity.OnRecoil(IsScopedIn);
                hitDetector.CheckHit(hitDetectorInfo, BoundEntity.GetRealDamageValue());
            }

            SetShootStatus(shouldShoot);
        }
        
        public bool CheckHit(HitData data)
        {
            return data.Hurtbox.Owner != gameObject;
        }
        
        public override void HitResponse(HitData data) {
            // TODO: Phase out old Particle System
            if (isHitVFX)
            {
                hitVFXSystem.SetVector3("StartPosition", data.HitPoint);
                hitVFXSystem.Play();
            }
            else
            {
                Instantiate(hitParticlePrefab, data.HitPoint, Quaternion.identity);
            }
        }
        
        // Item/Holding Functions
        protected override void OnStartAbsorb() {
           
        }

        public override void OnItemStartUse() {
            
        }

        public override void OnItemStopUse() {
            
        }
    }
}