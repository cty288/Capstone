using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Temporary
{
    public class PlayerController : MonoBehaviour, IHitResponder, IHurtResponder
    {
        //todo: entity data
        public int maxHealth = 100;
        //todo: entity data
        public int curHealth = 100;

        [SerializeField] private int m_damage = 10;
        public int Damage => m_damage;
 
        private List<GameObject> hitObjects = new List<GameObject>();
        
        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction>(Faction.Friendly);



        public bool CheckHit(HitData data)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckHurt(HitData data)
        {
            throw new System.NotImplementedException();
        }

        public void HitResponse(HitData data)
        {
            throw new System.NotImplementedException();
        }

        public void HurtResponse(HitData data) {
            throw new System.NotImplementedException();
        }

        public void TakeDamage(int damage)
        {
            curHealth -= damage;
            Debug.Log("Player takes " + damage + " damage.");
        }
    }
}
