using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Temporary;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.RunTimeTests.TestCollision
{
    public class Test_HurtResponder : MonoBehaviour, IHurtResponder
    {
        public TestEnity entity;

        [SerializeField] private List<HurtBox> hurtBoxes = new List<HurtBox>();
        [SerializeField] private Rigidbody rb;
        
        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction>(Faction.Hostile);


        private void Start()
        {
            entity = gameObject.GetComponent<TestEnity>();
            hurtBoxes = new List<HurtBox>(GetComponentsInChildren<HurtBox>());
            foreach (HurtBox hurtBox in hurtBoxes)
            {
                hurtBox.HurtResponder = this;
            }
        }

        public bool CheckHurt(HitData data)
        {
            return true;
        }

        public void HurtResponse(HitData data) {
            if (data.Attacker.IsSameFaction(this))
            {
                Debug.Log("is same faction");
                return;
            }
            
            Debug.Log("Hurt Response: Took " + data.Damage + " damage.");
            
            entity.TakeDamage(data.Damage);
            Vector3 force = -data.HitNormal * 10;
            Vector3 point = data.HitPoint;
            // rb.AddForceAtPosition(force, point, ForceMode.Impulse);
        }
    }
}
