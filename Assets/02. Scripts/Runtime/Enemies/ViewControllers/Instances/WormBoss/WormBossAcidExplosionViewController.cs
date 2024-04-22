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
        public GameObject acidDOTPrefab;
        private SafeGameObjectPool pool;

        private float acidDuration;
        private float acidTickInterval;
        
        private void Start()
        {
            pool = GameObjectPoolManager.Singleton.CreatePool(acidDOTPrefab, 3, 9);
        }
        
        public void SetData(float acidDuration, float acidTickInterval) {
            this.acidDuration = acidDuration;
            this.acidTickInterval = acidTickInterval;
        }
        
        protected override void OnHitResponse(HitData data)
        {
            var acidGO = pool.Allocate();
            acidGO.transform.position = data.HitPoint;
            acidGO.transform.rotation = Quaternion.Euler(data.HitNormal);
                
            var acidVC = acidGO.GetComponent<WormBossAcidAreaViewController>();
            acidVC.Init(Faction.Hostile, Damage, gameObject, owner, 50f);
            acidVC.SetData(acidDuration, acidTickInterval);
            
            GameObjectPoolManager.Singleton.Recycle(gameObject);
        }

        protected override void OnBulletRecycled() { }

        // private async UniTask PlayEffect()
        // {
        //     await UniTask.WaitForSeconds(1f);
        //
        //     explosionHitBox.enabled = false;
        //     
        //     dotHitBox.enabled = true;
        //     dotHitBox.StartCheckingHits(Damage);
        //     
        //     await UniTask.WaitForSeconds(duration);
        //     GameObjectPoolManager.Singleton.Recycle(gameObject);
        // }
    }
}
