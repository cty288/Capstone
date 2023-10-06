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
        
        public void Init(Faction faction, int damage) {
            CurrentFaction.Value = faction;
            Damage = damage;
        }

        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
        public void OnKillDamageable(IDamageable damageable) {
            
        }

        public void OnDealDamage(IDamageable damageable, int damage) {
            
        }
    }
}
