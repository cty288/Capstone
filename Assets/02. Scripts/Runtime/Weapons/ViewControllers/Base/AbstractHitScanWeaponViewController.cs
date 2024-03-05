using System.Linq;
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
        public VisualEffect[] BulletVFXInHand { get; }
        
        public VisualEffect[] BulletVFXAll { get; }

        public void ResetBulletVFX();
        public void SetBulletVFX(VisualEffect[] vfxIn, VisualEffect[] vfxOut);
    }
    
    public abstract class AbstractHitScanWeaponViewController<T> : AbstractWeaponViewController<T>, IHitResponder, IHitScanWeaponVFX
        where T : class, IWeaponEntity, new() {
        
        [Header("Aesthetic")]
        public VisualEffect[] bulletVFX;
        public VisualEffect[] bulletVFXInHand;
        private VisualEffect[] bulletVFXAll;
        protected HitDetectorInfo hitDetectorInfo;

        public VisualEffect[] BulletVFX => bulletVFX;
        public VisualEffect[] BulletVFXInHand => bulletVFXInHand;
        
        public VisualEffect[] BulletVFXAll {
            get
            {
                if (bulletVFXAll == null)
                {
                    bulletVFXAll = bulletVFX.Concat(bulletVFXInHand).ToArray();
                }
                return bulletVFXAll;
            }
        }

        public void ResetBulletVFX()
        {
            if (hitDetector is HitScan hs) hs.VFX = bulletVFXAll;
        }

        public void SetBulletVFX(VisualEffect[] vfxIn, VisualEffect[] vfxOut)
        {
            foreach (var v in vfxIn)
            {
                var t = v.transform;
                t.parent = bulletVFXInHand[0].transform;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
            foreach (var v in vfxOut)
            {
                var t = v.transform;
                t.parent = bulletVFX[0].transform;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
            var vfx = vfxIn.Concat(vfxOut).ToArray();
            if (hitDetector is HitScan hs) hs.VFX = vfx;
        }

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
    }
}