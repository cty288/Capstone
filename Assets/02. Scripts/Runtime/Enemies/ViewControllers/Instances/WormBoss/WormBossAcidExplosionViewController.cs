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
    public class WormBossAcidExplosionViewController : AbstractExplosionViewController
    {
        public ExplosionHitBox explosionHitBox;
        public DotHitBox dotHitBox;
        public float duration = 5f;

        public override void Init(Faction faction, int damage, float size, GameObject bulletOwner, ICanDealDamage owner) {
            base.Init(faction, damage, size, bulletOwner, owner);
            PlayEffect();
        }

        protected override void OnHitResponse(HitData data)
        {
            
        }

        protected override void OnBulletRecycled() {
            StopAllCoroutines();
        }

        private async UniTask PlayEffect()
        {
            await UniTask.WaitForSeconds(1f);

            explosionHitBox.enabled = false;
            dotHitBox.enabled = true;
            
            await UniTask.WaitForSeconds(duration);
            GameObjectPoolManager.Singleton.Recycle(gameObject);
        }
    }

}
