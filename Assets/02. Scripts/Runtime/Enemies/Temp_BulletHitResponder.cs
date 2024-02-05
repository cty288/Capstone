using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Enemies
{
    public class Temp_BulletHitResponder : MonoBehaviour, IHitResponder
    {
        public GameObject boss1;
        public int m_damage = 10;
        
        // Start is called before the first frame update
        void Start()
        {
            GetComponent<HitBox>().HitResponder = this;
            Destroy(gameObject, 5f);
            GetComponent<HitBox>().StartCheckingHits(m_damage);
        }

       

        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Hostile);
        public void OnKillDamageable(IDamageable damageable) {
            
        }

        public void OnDealDamage(IDamageable damageable, int damage) {
            
        }

        public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

        Action<IDamageable, int> ICanDealDamage.OnDealDamageCallback {
            get => _onDealDamageCallback;
            set => _onDealDamageCallback = value;
        }

        Action<IDamageable> ICanDealDamage.OnKillDamageableCallback {
            get => _onKillDamageableCallback;
            set => _onKillDamageableCallback = value;
        }

        public ICanDealDamage ParentDamageDealer => null;

        /*ublic ICanDealDamageRootEntity RootDamageDealer { get; }
                public ICanDealDamageRootViewController RootViewController { get; }p*/

        public int Damage => m_damage;
        public List<GameObject> hitObjects= new List<GameObject>();
        private Action<IDamageable, int> _onDealDamageCallback;
        private Action<IDamageable> _onKillDamageableCallback;

        public bool CheckHit(HitData data) {
            if (data.Hurtbox.Owner == boss1) { return false; }
            else { return true; }
        }

        public void HitResponse(HitData data)
        {
            hitObjects.Add(data.Hurtbox.Owner);
            Destroy(gameObject);
        }

        public HitData OnModifyHitData(HitData data) {
            return data;
        }
    }
}

