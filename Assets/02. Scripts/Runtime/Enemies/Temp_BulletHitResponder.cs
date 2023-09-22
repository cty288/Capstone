using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
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
            GetComponent<HitBox>().StartCheckingHits();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Hostile);
        public int Damage => m_damage;
        public List<GameObject> hitObjects= new List<GameObject>();
        public bool CheckHit(HitData data)
        {
            if (data.Hurtbox.Owner == boss1) { return false; }
            else { return true; }
        }

        public void HitResponse(HitData data)
        {
            hitObjects.Add(data.Hurtbox.Owner);
            Destroy(gameObject);
        }
    }
}

