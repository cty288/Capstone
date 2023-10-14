using MikroFramework.Architecture;
using Runtime.Player;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Base
{
    public abstract class AbstractProjectileWeaponViewController<T> : AbstractWeaponViewController<T>, IHitResponder
        where T : class, IWeaponEntity, new() {
        
        protected override void OnEntityStart()
        {
            base.OnEntityStart();
            
            playerModel = this.GetModel<IGamePlayerModel>();
        }
        
        protected override IHitDetector OnCreateHitDetector() {
            return null;
        }



        public virtual void Shoot() {
            BoundEntity.OnRecoil(IsScopedIn);
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