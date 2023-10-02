using Framework;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.BindableProperty;
using Polyglot;
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
    public class Boss1Entity : BossEntity<Boss1Entity>
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "Boss1";
        

        [field: ES3Serializable] public BindableProperty<bool> ShellClosed { get; } = new BindableProperty<bool>(true);
        
        public override void OnRecycle() {

        }

        protected override void OnEnemyRegisterAdditionalProperties() {
            
        }

        protected override string OnGetDescription(string defaultLocalizationKey) {
            return Localization.Get(defaultLocalizationKey);
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return new[] {
                new AutoConfigCustomProperty("shellHealthInfo"),
                new AutoConfigCustomProperty("damages")
            };
        }
        
        public void ChangeShellStatus(bool newStatus) {
            
            ShellClosed.Value = newStatus;
        }

        
    }
    public class Boss1 : AbstractBossViewController<Boss1Entity>
    {
        
        public int MaxShellHealth { get; }
        
        public int CurrentShellHealth { get; }
        [Header("HitResponder_Info")]
        public Animator animator;
        
        public NavMeshAgent agent;
        
        
        
        [SerializeField] private HitBox hitbox_roll;

       
        private HitDetectorInfo hitDetectorInfo;
        private bool deathAnimationEnd = false;
        

        private Collider hardCollider;
        
        [Header("Shell")]
        [SerializeField]
        private Collider shellCollider;
        
        [SerializeField] private Transform shellHealthBarSpawnTransform;

        protected override void Awake() {
            base.Awake();
           
        }

        protected override MikroAction WaitingForDeathCondition() {
            transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => {
                deathAnimationEnd = true;
            });
            
            return UntilAction.Allocate(() => deathAnimationEnd);
        }

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
          
        }

        protected override void OnEntityStart()
        {
            Debug.Log("start");
            //binding
            BindCustomData<int>("CurrentShellHealth", "shellHealthInfo", "info",info=>info.CurrentHealth);
            BindCustomData<int>("MaxShellHealth", "shellHealthInfo", "info",info=>info.MaxHealth);
            
            
            BoundEntity.ShellClosed.RegisterWithInitValue(OnShellClosedChanged).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            //Animation-related.
            //animator = GetComponent<Animator>();


            //Collision-related.
            if (hitbox_roll) {
                hitbox_roll.HitResponder = this;
            }

            hardCollider = GetComponent<Collider>();
            hitDetectorInfo = new HitDetectorInfo();
            CurrentFaction.Value = Faction.Hostile;

            BoundEntity.IsInvincible.Value = true;
            SpawnShellHealthBar();
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer) {
            Debug.Log($"Boss 1 Take damage: {damage}. Boss 1 current health: {currenthealth}");
        }

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<Boss1Entity> builder)
        {
            return builder.
                FromConfig()
                //.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
                .Build();
        }
        
        protected void OnShellClosedChanged(bool oldValue,bool newValue) {
            Debug.Log("changed to" + newValue);
            if (CurrentShellHealth <= 0 && !newValue) {
                animator.CrossFade("OpenImmediately", 0.1f);
            }
            animator.SetBool("ShellClosed",newValue);
        }
        // private void Update()
        // {
        //
        // }

        protected override void OnAnimationEvent(string eventName) {
            switch (eventName)
            {
                case "ShellOpen":
                    BoundEntity.IsInvincible.Value = false;
                    UnSpawnShellHealthBar();
                    break;
                case "ShellClose":
                    BoundEntity.IsInvincible.Value = true;
                    SpawnShellHealthBar();
                    break;
                default:
                    break;
            }
        }

        private void SpawnShellHealthBar() {
            HealthBar healthBar = SpawnCrosshairResponseHUDElement(shellHealthBarSpawnTransform, "Boss1ShellHealthBar",
                HUDCategory.HealthBar, false).GetComponent<HealthBar>();

            healthBar.OnSetEntity(BoundEntity.GetCustomDataValue<HealthInfo>("shellHealthInfo", "info"), BoundEntity);
        }
        
        private void UnSpawnShellHealthBar() {
            DespawnHUDElement(shellHealthBarSpawnTransform, HUDCategory.HealthBar);
        }
        
        
        public void ClearHitObjects() {
            hitObjects.Clear();
        }

        public override void HurtResponse(HitData data)
        {
            Debug.Log("hurt response");
            if (BoundEntity.ShellClosed)
            {
                IBindableProperty shellHp = BoundEntity.GetCustomDataValue("shellHealthInfo", "info");


                if (shellHp.Value.CurrentHealth > 0) {
                    if (shellHp.Value.CurrentHealth - data.Damage <= 0) {
                        shellHp.Value = new HealthInfo(shellHp.Value.MaxHealth,0);
                    }
                    else {
                        shellHp.Value = new HealthInfo(shellHp.Value.MaxHealth, shellHp.Value.CurrentHealth - data.Damage);
                    }
                }
               
                
                Debug.Log("Shell has taken" + data.Damage +"damage" + " Shell now has" + shellHp.Value.CurrentHealth + "hp");
            }
            
            BoundEntity.TakeDamage(data.Damage, data.Attacker);
        }

        public void ChangeShellStatus(bool newStatus) {
            BindableProperty<bool> shellStatus = BoundEntity.ShellClosed;
            shellStatus.Value = newStatus;
            shellCollider.enabled = !newStatus;
            hardCollider.enabled = newStatus;
        }


        public override void OnRecycled() {
            base.OnRecycled();
            transform.localScale = Vector3.one;
            deathAnimationEnd = false;
            shellCollider.enabled = true;
            hardCollider.enabled = true;
        }
    }
}

