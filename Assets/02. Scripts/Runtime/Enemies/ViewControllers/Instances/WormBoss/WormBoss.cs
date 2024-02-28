using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.ViewControllers.Base;
using Runtime.Utilities.Collision;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Runtime.Enemies
{
    public class WormBossEntity : BossEntity<WormBossEntity>
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "WormBoss";
        public List<GameObject> missileFirePosList;
        
        protected override void OnEntityStart(bool isLoadedFromSave)
        {
            Debug.Log("worm start");
        }

        public override void OnRecycle()
        {
            base.OnRecycle();
        }
        
        public void InitializeMissileFirePosList(List<GameObject> missileFirePosList)
        {
            this.missileFirePosList = missileFirePosList;
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
                new AutoConfigCustomProperty("fallAttack"),
                new AutoConfigCustomProperty("arc"),
                new AutoConfigCustomProperty("missileAttack"),
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
        [SerializeField] private GameObject rig;
        [SerializeField] private GameObject vfx;
        [SerializeField] private GameObject wormSphere;
        [SerializeField] private GameObject wormPlane;
        

        [SerializeField] private HitBox fallHitBox;

        [SerializeField] private GameObject sandParticleVFX;
        [HideInInspector] public GameObjectPool sandParticlePool;
        [SerializeField] private GameObject deathExplosionVFX;
        private GameObjectPool deathExplosionPool;
        [SerializeField] private GameObject deathExplosion2VFX;
        private GameObjectPool deathExplosion2Pool;
        
        public List<GameObject> missileFirePosList;
        public List<GameObject> segments;
        
        private int fallAttackDamage;
        private float fallKnockbackForce;
        public bool inited = false;

        private bool finishDeathAnimation = false;
        
        protected override void Awake()
        {
            base.Awake();
            animator = GetComponentInChildren<Animator>(true);
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
            // director = gameObject.GetComponent<PlayableDirector>();
        }
        
        protected override void OnEntityStart()
        {
            agent.speed = Speed;
            fallAttackDamage = BoundEntity.GetCustomDataValue<int>("fallAttack", "damage").Value;
            fallKnockbackForce = BoundEntity.GetCustomDataValue<float>("fallAttack", "knockbackForce").Value;

            sandParticlePool = GameObjectPoolManager.Singleton.CreatePool(sandParticleVFX, 8, 24);
            deathExplosionPool = GameObjectPoolManager.Singleton.CreatePool(deathExplosionVFX, 30, 60);
            deathExplosion2Pool = GameObjectPoolManager.Singleton.CreatePool(deathExplosion2VFX, 10, 20);
            
            BoundEntity.InitializeMissileFirePosList(missileFirePosList);
            
            if (fallHitBox)
            {
                fallHitBox.HitResponder = this;
            }
            inited = true;
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
        
        protected override MikroAction WaitingForDeathCondition()
        {
            return UntilAction.Allocate(() => !model.gameObject.activeInHierarchy || finishDeathAnimation);
        }
        
        protected override void OnEntityDie(ICanDealDamage damagedealer) {
            base.OnEntityDie(damagedealer);
            behaviorTree.enabled = false;
            agent.enabled = false;
            rb.isKinematic = true;
            
            
            PlayDeathAnimation();
        }

        private async UniTask PlayDeathAnimation()
        {
            await UniTask.WaitForSeconds(1f);

            for (int j = missileFirePosList.Count - 1; j >= 0; j--)
            {
                // only spawn explosion every 3rd position
                Transform spawnPos = missileFirePosList[j].transform;
                var e = deathExplosionPool.Allocate();
                e.transform.position = spawnPos.position + transform.forward * 1.2f;
            
                await UniTask.WaitForSeconds(0.1f);
            }

            await UniTask.WaitForSeconds(2f);

            model.gameObject.SetActive(false);
            rig.gameObject.SetActive(false);
            vfx.gameObject.SetActive(false);
            
            wormSphere.transform.parent = gameObject.transform;
            wormPlane.transform.parent = gameObject.transform;
            wormSphere.gameObject.SetActive(false);
            wormPlane.gameObject.SetActive(false);
            
            foreach (var segment in segments)
            {
                var e = deathExplosion2Pool.Allocate();
                e.transform.position = segment.transform.position;
            }
            
            await UniTask.WaitForSeconds(1f);
            
            finishDeathAnimation = true;
        }

        public override void OnRecycled()
        {
            base.OnRecycled();
            
            agent.enabled = true;
            behaviorTree.enabled = true;
            model.gameObject.SetActive(true);
            rig.gameObject.SetActive(true);
            vfx.gameObject.SetActive(true);
        }
    }
}
