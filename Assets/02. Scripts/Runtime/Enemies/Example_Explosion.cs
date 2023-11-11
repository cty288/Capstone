using System.Collections;
using System.Collections.Generic;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Enemies
{
    public class Example_Explosion : AbstractExplosionViewController
    {
        
        ParticleSystem[] particleSystems;
        protected Dictionary<ParticleSystem, Vector2> originalStartSize = new Dictionary<ParticleSystem, Vector2>();
        protected override void Awake() {
            base.Awake();
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var particleSystem in particleSystems) {
                originalStartSize.Add(particleSystem, new Vector2(particleSystem.main.startSize.constantMin, particleSystem.main.startSize.constantMax));
            }
        }

        public override void Init(Faction faction, int damage, float size, GameObject bulletOwner, ICanDealDamage owner) {
            base.Init(faction, damage, size, bulletOwner, owner);
            
            foreach (var particleSystem in particleSystems) {
                var main = particleSystem.main;
                main.startSize = new ParticleSystem.MinMaxCurve(size * originalStartSize[particleSystem].x, size * originalStartSize[particleSystem].y);
                //particleSystem.main = main;
            }
        }

        protected override void OnHitResponse(HitData data)
        {
            
        }

        protected override void OnBulletRecycled()
        {
            StopAllCoroutines();
        }
    }

}
