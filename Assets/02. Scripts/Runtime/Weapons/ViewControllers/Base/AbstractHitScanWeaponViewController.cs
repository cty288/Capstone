using MikroFramework.Architecture;
using Runtime.Player;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Base
{
    public abstract class AbstractHitScanWeaponViewController<T> : AbstractWeaponViewController<T>, IHitResponder
        where T : class, IWeaponEntity, new() {
        
        [Header("Aesthetic")]
        public TrailRenderer trailRenderer;
        
        private HitDetectorInfo hitDetectorInfo;
        
        
        protected override void OnEntityStart()
        {
            base.OnEntityStart();
            
            
            
            hitDetectorInfo = new HitDetectorInfo
            {
                camera = cam,
                layer = layer,
                launchPoint = trailRenderer.transform,
                weapon = BoundEntity
            };
        }
        
        protected override IHitDetector OnCreateHitDetector() {
            return new HitScan(this, CurrentFaction.Value, trailRenderer);
        }
        
        public virtual void SetShoot(bool shouldShoot) {
            if (shouldShoot) {
                crossHairViewController?.OnShoot();
                BoundEntity.OnRecoil(IsScopedIn);
                hitDetector.CheckHit(hitDetectorInfo, BoundEntity.GetBaseDamage().RealValue);
            }

            SetShootStatus(shouldShoot);
        }
        
        public bool CheckHit(HitData data)
        {
            return data.Hurtbox.Owner != gameObject;
        }
        
        public override void HitResponse(HitData data) {
            Instantiate(hitParticlePrefab, data.HitPoint, Quaternion.identity);
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