using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using Runtime.Player;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

namespace Runtime.Weapons.ViewControllers.Base
{
    public interface IHitScanWeaponVFX
    {
        public const int BULLET_VFX_COUNT = 2;
        public VisualEffect[] BulletVFX { get; }
        
        public void SetHitVFX(VisualEffect vfx);
        public void ResetHitVFX();
    }
    
    public abstract class AbstractHitScanWeaponViewController<T> : AbstractWeaponViewController<T>, IHitResponder, IHitScanWeaponVFX
        where T : class, IWeaponEntity, new() {
        
        [Header("Aesthetic")]
        public VisualEffect[] bulletVFX;
        protected VisualEffect[] originalBulletVFX;
        protected HitDetectorInfo hitDetectorInfo;

        public VisualEffect[] BulletVFX => bulletVFX;

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

            originalBulletVFX = new VisualEffect[IHitScanWeaponVFX.BULLET_VFX_COUNT];
            for (int i = 0; i < originalBulletVFX.Length; i++)
            {
                originalBulletVFX[i] = bulletVFX[i];
            }
            
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
            hitVFXSystem.SetVector3("StartPosition", data.HitPoint);
            hitVFXSystem.SetVector3("HitNormal", data.HitNormal);
            hitVFXSystem.Play();
        }
        
        public void SetHitVFX(VisualEffect vfx)
        {
            throw new System.NotImplementedException();
            bulletVFX[0] = vfx;
        }

        public void ResetHitVFX()
        {
            throw new System.NotImplementedException();
        }
    }
}