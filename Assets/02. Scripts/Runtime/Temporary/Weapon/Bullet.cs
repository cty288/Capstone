using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Temporary.Weapon
{
    public class Bullet : MonoBehaviour, IHitResponder
    {
        [SerializeField] private int m_damage = 0;

        [Header("Hitboxes")]
        [SerializeField] private HitBox hitbox;

        [Header("Hit Effects")]
        [SerializeField] private GameObject explosionPrefab;

        public int Damage => m_damage;

        private void Start()
        {
            hitbox.HitResponder = this;
            Destroy(gameObject, 3f);
        }

        private void FixedUpdate()
        {
            // hitbox.CheckHit();
        }

        public bool CheckHit(HitData data)
        {
            if (data.Hurtbox.Owner == gameObject) { return false; }
            else { return true; }
        }

        public void HitResponse(HitData data)
        {
            Debug.Log("Bullet Hit Response: " + data.HitPoint);
            Instantiate(explosionPrefab, data.HitPoint, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
