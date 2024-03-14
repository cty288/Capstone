using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Enemies
{
    public class WormBossAcidAreaViewController : AbstractDotBulletViewController
    {
        private float acidDuration;
        
        private DotHitBox dotHitBox;

        private void Start()
        {
            dotHitBox = gameObject.GetComponent<DotHitBox>();
        }
        
        public void SetData(float acidDuration, float acidTickInterval) {
            this.acidDuration = acidDuration;
            
            if(dotHitBox == null)
                dotHitBox = gameObject.GetComponent<DotHitBox>();
            dotHitBox.dotTick = acidTickInterval;

            PlayEffect();
        }
        
        protected override void OnHitResponse(HitData data)
        {
        }

        protected override void OnHitObject(Collider other)
        {
        }

        protected override void OnBulletReachesMaxRange()
        {
        }

        protected override void OnBulletRecycled()
        {
        }
        
        protected override void OnTriggerEnter(Collider other)
        {
            if (!other.isTrigger)
            {
                Rigidbody rootRigidbody = other.attachedRigidbody;
                GameObject hitObj =
                    rootRigidbody ? rootRigidbody.gameObject : other.gameObject;

                if (hitObj != null && hitObj.transform == bulletOwner.transform)
                {
                    return;
                }
                if (hitObj.TryGetComponent<IBelongToFaction>(out var belongToFaction))
                {
                    if (belongToFaction.CurrentFaction.Value == CurrentFaction.Value && penetrateSameFaction)
                    {
                        return;
                    }
                }

                OnHitObject(other);
            }
        }
        
        private async UniTask PlayEffect()
        {
            await UniTask.WaitForSeconds(1f);
        
            dotHitBox.enabled = true;
            dotHitBox.StartCheckingHits(Damage);
            await UniTask.WaitForSeconds(acidDuration);

            GameObjectPoolManager.Singleton.Recycle(gameObject);
        }
    }
}
