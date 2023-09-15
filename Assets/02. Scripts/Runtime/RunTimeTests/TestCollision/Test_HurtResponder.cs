using System.Collections.Generic;
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
            
            // Debug.Log("Hurt Response: Took " + data.Damage + " damage.");
            entity.TakeDamage(data.Damage);
            Vector3 force = -data.HitNormal * 10;
            Vector3 point = data.HitPoint;
            // rb.AddForceAtPosition(force, point, ForceMode.Impulse);
        }
    }
}
