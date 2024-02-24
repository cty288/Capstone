using Framework;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using Runtime.Enemies.ViewControllers.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Enemies;
using Runtime.UI.NameTags;
using Runtime.Utilities;
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.Collision;
using UnityEngine;
using UnityEngine.AI;

using PropertyName = Runtime.DataFramework.Properties.PropertyName;
namespace Runtime.Enemies
{
    public class WormBossEntity : BossEntity<WormBossEntity>
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "WormBoss";
        
        protected override void OnEntityStart(bool isLoadedFromSave)
        {
            Debug.Log("worm start");
        }

        public override void OnRecycle()
        {
            base.OnRecycle();
        }
        
        protected override void OnInitModifiers(int rarity, int level) {}
        
        protected override void OnEnemyRegisterAdditionalProperties() {}

        protected override string OnGetDescription(string defaultLocalizationKey) {
            return Localization.Get(defaultLocalizationKey);
        }

        protected override ICustomProperty[] OnRegisterCustomProperties() {
            return new[] {
                new AutoConfigCustomProperty("laserBeam"),
                new AutoConfigCustomProperty("acidAttack"),
                new AutoConfigCustomProperty("rapidFire"),
                new AutoConfigCustomProperty("entity"),
                new AutoConfigCustomProperty("fallAttack")
            };
        }
    }
    
    public class WormBoss : AbstractBossViewController<WormBossEntity>
    {
        [BindCustomData("entity", "speed")]
        public float Speed { get;}
        [BindCustomData("entity", "closeRange")]
        public float CloseRange { get;}
        
        private Animator animator;
        private NavMeshAgent agent;
        private Rigidbody rb;

        [SerializeField] private GameObject model;

        [SerializeField] private HitBox fallHitBox;

        [SerializeField] private GameObject sandParticleVFX;
        private GameObjectPool sandParticlePool;
        
        private int fallAttackDamage;
        private float fallKnockbackForce;
        
        protected override void Awake()
        {
            base.Awake();
            animator = GetComponentInChildren<Animator>(true);
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
        }
        
        protected override void OnEntityStart()
        {
            agent.speed = Speed;
            fallAttackDamage = BoundEntity.GetCustomDataValue<int>("fallAttack", "damage").Value;
            fallKnockbackForce = BoundEntity.GetCustomDataValue<float>("fallAttack", "knockbackForce").Value;

            sandParticlePool = GameObjectPoolManager.Singleton.CreatePool(sandParticleVFX, 8, 16);
            
            if (fallHitBox)
            {
                fallHitBox.HitResponder = this;
            }
        }

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer)
        {
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer)
        {
        }

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<WormBossEntity> builder)
        {
            return builder.
                FromConfig()
                .Build();
        }
        
        private void ClearHitObjects() {
            hitObjects.Clear();
        }
        
        public override void HitResponse(HitData data)
        {
            base.HitResponse(data);
            print($"WORM BOSS HIT RESPONSE: {data.Hurtbox}");
            
            if (fallHitBox.isActiveAndEnabled && data.Hurtbox != null && data.Hurtbox.Owner.CompareTag("Player"))
            {
                Vector3 dir = data.Hurtbox.Owner.transform.position - transform.position;
                dir.y = 0;
                //make it 45 degrees from the ground
                dir = Quaternion.AngleAxis(45, Vector3.Cross(dir, Vector3.up)) * dir;
                dir.Normalize();
                data.Hurtbox.Owner.GetComponent<Rigidbody>().AddForce(dir * fallKnockbackForce, ForceMode.Impulse);
            }
        }
        
        protected override void OnAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case "EnableGravity":
                    rb.useGravity = true;
                    ClearHitObjects();
                    fallHitBox.gameObject.SetActive(true);
                    fallHitBox.StartCheckingHits(fallAttackDamage);
                    break;
                case "DisableGravity":
                    rb.useGravity = false;
                    fallHitBox.StopCheckingHits();
                    fallHitBox.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
        
        protected override MikroAction WaitingForDeathCondition() {

            return UntilAction.Allocate(() => !model.gameObject.activeInHierarchy ||
                                              (animator.GetCurrentAnimatorStateInfo(0).IsName("Die") &&
                                               animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f));
        }
        
        protected override void OnEntityDie(ICanDealDamage damagedealer) {
            base.OnEntityDie(damagedealer);
            model.gameObject.SetActive(true);
            behaviorTree.enabled = false;
            // animator.SetBool("Die", true);
            // animator.CrossFadeInFixedTime("Die", 0.1f);
            // rb.isKinematic = false;
            // rb.useGravity = true;
            agent.enabled = false;
        }

        public override void OnRecycled()
        {
            base.OnRecycled();
            
            transform.localScale = Vector3.one;
            agent.enabled = true;
            behaviorTree.enabled = true;
            model.gameObject.SetActive(true);
        }
    }
}
