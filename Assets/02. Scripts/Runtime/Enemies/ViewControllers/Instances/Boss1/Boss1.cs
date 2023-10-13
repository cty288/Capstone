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

        protected override void OnEntityStart(bool isLoadedFromSave) {
            
        }

        public override void OnRecycle() {

        }
        protected override void OnInitModifiers(int rarity, int level) {
            
        }

        public override int OnGetRealSpawnWeight(int level, int baseWeight) {
            return level * baseWeight;
        }

        public override int OnGetRealSpawnCost(int level, int rarity, int baseCost) {
            return level * baseCost;
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
                new AutoConfigCustomProperty("damages"),
                new AutoConfigCustomProperty("ranges"),
                new AutoConfigCustomProperty("waitTimes")
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

        [BindCustomData("ranges", "closeRange")]
        public float CloseRange { get;}
        
        [BindCustomData("ranges", "midRange")]
        public float MidRange { get; }
        
        [BindCustomData("ranges", "longRange")]
        public float LongRange { get;}

        [BindCustomData("ranges", "meleeRange")]
        public float MeleeRange { get; }
       
        [BindCustomData("waitTimes","meleeWait")]
        public float MeleeWait { get; }
        [BindCustomData("waitTimes","rapidFireWait")]
        public float RapidFireWait { get; }
        [BindCustomData("waitTimes","rangedAOEWait")]
        public float RangedAOEWait { get; }
        [BindCustomData("waitTimes","rollWait")]
        public float RollWait { get; }
        
        private HitDetectorInfo hitDetectorInfo;
        private bool deathAnimationEnd = false;
        

        [SerializeField]
        private Collider hardCollider;
        
        [Header("Shell")]
        [SerializeField]
        private Collider shellCollider;
        
        [SerializeField] private Transform shellHealthBarSpawnTransform;

        [SerializeField] private HitBox slamHitBox;
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

            if (slamHitBox)
            {
                slamHitBox.HitResponder = this;

            }
            hitDetectorInfo = new HitDetectorInfo();
            CurrentFaction.Value = Faction.Hostile;
            
            BoundEntity.IsInvincible.Value = true;
            SpawnShellHealthBar();
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer) {
            if (BoundEntity.IsInvincible) {
                showDamageNumber = false;
            }
            else {
                showDamageNumber = true;
            }
        }

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<Boss1Entity> builder)
        {
            return builder.
                FromConfig()
                //.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
                .Build();
        }
        
        protected void OnShellClosedChanged(bool oldValue,bool newValue) {
            //GetComponent<>()
            
            Debug.Log("changed to" + newValue);
            shellCollider.enabled = newValue;
            hardCollider.enabled = newValue;
            if (CurrentShellHealth <= 0 && !newValue) {
                animator.CrossFade("OpenImmediately", 0.05f);
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
                case "ClearHits":
                    hitObjects.Clear();
                    break;
                case "MeleeStart":
                    ClearHitObjects();
                    slamHitBox.gameObject.SetActive(true);
                    slamHitBox.StartCheckingHits(BoundEntity.GetCustomDataValue<int>("damages","meleeDamage").Value);
                    break;
                case "MeleeFinish":
                    
                    slamHitBox.StopCheckingHits();
                    slamHitBox.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        private void SpawnShellHealthBar() {
            HealthBar healthBar = SpawnCrosshairResponseHUDElement(shellHealthBarSpawnTransform, "Boss1ShellHealthBar",
                HUDCategory.HealthBar, false).Item1.GetComponent<HealthBar>();

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
            int originalDamage = data.Damage;
            //base.HurtResponse(data);
            Debug.Log("hurt response");
            if (BoundEntity.ShellClosed)
            {
                IBindableProperty shellHp = BoundEntity.GetCustomDataValue("shellHealthInfo", "info");
               
                if (shellHp.Value.CurrentHealth > 0) {
                    if (shellHp.Value.CurrentHealth -originalDamage <= 0) {
                        shellHp.Value = new HealthInfo(shellHp.Value.MaxHealth,0);
                    }
                    else {
                        shellHp.Value = new HealthInfo(shellHp.Value.MaxHealth, shellHp.Value.CurrentHealth - originalDamage);
                    }
                }

                DamageNumberHUD.Singleton.SpawnHUD(data?.HitPoint ?? transform.position, originalDamage);
                
                Debug.Log("Shell has taken" + originalDamage +"damage" + " Shell now has" + shellHp.Value.CurrentHealth + "hp");
            }

            BoundEntity.TakeDamage(data.Damage, data.Attacker, data);
        }

        public void ChangeShellStatus(bool newStatus) {
            BindableProperty<bool> shellStatus = BoundEntity.ShellClosed;
            shellStatus.Value = newStatus;

        }
        
        
        public void MeleeAttack()
        {
            
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

