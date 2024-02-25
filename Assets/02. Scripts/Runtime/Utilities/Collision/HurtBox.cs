using UnityEngine;

namespace Runtime.Utilities.Collision
{
    [RequireComponent(typeof(Collider))]
    public class HurtBox : MonoBehaviour, IHurtbox
    {
        [SerializeField] private bool m_active = true;
        [SerializeField] private GameObject m_owner = null;

        private IHurtResponder m_hurtResponder;
        [SerializeField] private float m_damageMultiplier = 1f;

        public bool Active => m_active;

        public GameObject Owner => m_owner;

        public Transform Transform => transform;

        public IHurtResponder HurtResponder { get => m_hurtResponder; set => m_hurtResponder = value; }
        public float DamageMultiplier { get => m_damageMultiplier; set => m_damageMultiplier = value; }

        private void Awake() {
            if (m_owner == null)
                m_owner = GetComponent<Collider>().attachedRigidbody.gameObject;
        }
        
        public void SetOwner(GameObject owner)
        {
            m_owner = owner;
        }

        public bool CheckHit(HitData data)
        {
            if (m_hurtResponder == null)
                Debug.Log("hurt responder is null");

            return true;
        }
    }
}
