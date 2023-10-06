using MikroFramework.Architecture;
using Runtime.Player;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Base
{
    public abstract class AbstractHitScanWeaponViewController<T> : AbstractWeaponViewController<T>, IHitResponder
        where T : class, IWeaponEntity, new() {
        
        [Header("Auto Reload")]
        public bool autoReload = false;
        
        [Header("Layer Hit Mask")]
        public LayerMask layer;
        
        [Header("Aesthetic")]
        public TrailRenderer trailRenderer;
        public GameObject hitParticlePrefab;
        
        private HitDetectorInfo hitDetectorInfo;
        
        // general references
        protected Camera cam;
        protected DPunkInputs.PlayerActions playerActions;
        
        
        //status
        protected bool isScopedIn = false;
        protected bool isReloading = false;
        
        //timers
        protected float lastShootTime = 0f;
        protected float reloadTimer = 0f;
        
        
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
        
        public virtual void Shoot() {
            crossHairViewController?.OnShoot();
            BoundEntity.OnRecoil(isScopedIn);
            hitDetector.CheckHit(hitDetectorInfo, BoundEntity.GetBaseDamage().RealValue);
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