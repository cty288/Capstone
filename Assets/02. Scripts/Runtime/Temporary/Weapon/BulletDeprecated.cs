using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Temporary.Weapon
{
    
    //TODO: add bullet damage to gun config
    
    public class BulletDeprecated : MonoBehaviour, IHitResponder, IBelongToFaction
    {
        //[SerializeField] private int m_damage = 0;

        [Header("Hitboxes")]
        [SerializeField] private HitBox hitbox;

        [Header("Hit Effects")]
        [SerializeField] private GameObject explosionPrefab;

        private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
        private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;


        public int Damage { get; protected set; }

        private void Start()
        {
            hitbox.HitResponder = this;
            Destroy(gameObject, 3f);
        }

        private void FixedUpdate() {
            // hitbox.CheckHit();
        }

        public bool CheckHit(HitData data)
        {
            if (data.Hurtbox.Owner == gameObject) { return false; }
            else { return true; }
        }

        public void HitResponse(HitData data) {
            Debug.Log("Bullet Hit Response: " + data.HitPoint);
            Instantiate(explosionPrefab, data.HitPoint, Quaternion.identity);
            Destroy(gameObject);
        }

        public HitData OnModifyHitData(HitData data) {
            return data;
        }

        public void Init(Faction faction, int damage) {
            CurrentFaction.Value = faction;
            Damage = damage;
        }

        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
        public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
            
        }

        public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
            
        }

        public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; }

        Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
            get => _onDealDamageCallback;
            set => _onDealDamageCallback = value;
        }

        Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
            get => _onKillDamageableCallback;
            set => _onKillDamageableCallback = value;
        }

        public ICanDealDamage ParentDamageDealer { get; }

      

        /*public ICanDealDamageRootEntity RootDamageDealer { get; }
        public ICanDealDamageRootViewController RootViewController { get; }*/
    }
}
