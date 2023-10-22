using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Temporary.Weapon
{
    public class Explosion : MonoBehaviour, IHitResponder
    {
        [SerializeField] private int m_damage = 20;
        [SerializeField] private ParticleSystem particle;

        [Header("Hitboxes")]
        [SerializeField] private HitBox hitbox;

        // [Header("Hit Effects")]
        // [SerializeField] private GameObject hitParticlePrefab;

        private List<GameObject> hitObjects = new List<GameObject>();

        public int Damage => m_damage;
        
        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction>(Faction.Neutral);

        public void OnKillDamageable(IDamageable damageable) {
            
        }

        public void OnDealDamage(IDamageable damageable, int damage) {
            
        }

        public ICanDealDamageRootEntity RootDamageDealer { get; }


        public void Start()
        {
            hitbox.HitResponder = this;
            // hitbox.CheckHit();
            Destroy(gameObject, particle.main.duration);
        }

        // private void FixedUpdate()
        // {
        //     hitbox.CheckHit();
        // }

        public bool CheckHit(HitData data)
        {
            if (data.Hurtbox.Owner == gameObject) { return false; }
            else if (hitObjects.Contains(data.Hurtbox.Owner)) { return false; }
            else { return true; }
        }

        public void HitResponse(HitData data)
        {
            // Destroy(gameObject);
            hitObjects.Add(data.Hurtbox.Owner);
            Debug.Log("Explosion Hit Response");
        }
    }
}
