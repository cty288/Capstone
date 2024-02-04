using System;
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
        private Action<IDamageable, int> _onDealDamageCallback;
        private Action<IDamageable> _onKillDamageableCallback;

        public int Damage => m_damage;
        
        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction>(Faction.Explosion);

        public void OnKillDamageable(IDamageable damageable) {
            
        }

        public void OnDealDamage(IDamageable damageable, int damage) {
            
        }

        public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; }

        Action<IDamageable, int> ICanDealDamage.OnDealDamageCallback {
            get => _onDealDamageCallback;
            set => _onDealDamageCallback = value;
        }

        Action<IDamageable> ICanDealDamage.OnKillDamageableCallback {
            get => _onKillDamageableCallback;
            set => _onKillDamageableCallback = value;
        }

        public ICanDealDamage ParentDamageDealer => null;

        /*
        public ICanDealDamageRootEntity RootDamageDealer { get; }
        public ICanDealDamageRootViewController RootViewController { get; }
        */


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

        public HitData OnModifyHitData(HitData data) {
            return data;
        }
    }
}
