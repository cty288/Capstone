using Framework;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.BindableProperty;
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
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.Collision;
using UnityEngine;
using UnityEngine.AI;

using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Enemies
{
    public class Boss1Entity : EnemyEntity<Boss1Entity>
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "Boss1";
        
        public override void OnRecycle()
        {

        }

        protected override void OnEnemyRegisterAdditionalProperties()
        {
            
        }

        protected override string OnGetDescription(string defaultLocalizationKey)
        {
            return null;
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return new[] {new AutoConfigCustomProperty("shellHealthInfo")};
        }

        
    }
    public class Boss1 : AbstractEnemyViewController<Boss1Entity>, IHurtResponder
    {
        
        public int MaxShellHealth { get; }
        
        public int CurrentShellHealth { get; }
        
        public bool ShellClosed { get; }
        [Header("HitResponder_Info")]
        [SerializeField] private int m_damage = 10;
        public Animator animator;
        public AnimationSMBManager animationSMBManager;
        public NavMeshAgent agent;
      
        
        
        protected override int GetCurrentHitDamage() {
            return m_damage; //TODO: this is temporary, the damage should be retrieved from the model according to the current attack.
        }

        [SerializeField] private HitBox hitbox_roll;

       
        private HitDetectorInfo hitDetectorInfo;
        
        
        protected override MikroAction WaitingForDeathCondition() {
            return UntilAction.Allocate(() => {
                if (Input.GetKeyDown(KeyCode.M)) {
                    Debug.Log("Boss 1 death animation ends.");
                    return true;
                }

                return false;
            });
        }

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEntityStart()
        {
            //binding
            BindCustomData<int>("CurrentShellHealth", "shellHealthInfo", "info",info=>info.CurrentHealth);
            BindCustomData<int>("MaxShellHealth", "shellHealthInfo", "info",info=>info.MaxHealth);
            BindCustomData<bool>("ShellClosed","shellHealthInfo","shellClosed");
            //Animation-related.
            // animator = GetComponent<Animator>();
            animationSMBManager = GetComponent<AnimationSMBManager>();
            animationSMBManager.Event.AddListener(OnAnimationEvent);

            //Collision-related.
            if (hitbox_roll) {
                hitbox_roll.HitResponder = this;
            }
           
            hitDetectorInfo = new HitDetectorInfo();
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

        protected void OnShellStatusChanged(bool oldValue,bool newValue)
        {
            Debug.Log("Shell status changed to:" + newValue);
        }
        private void Update()
        {

        }

        public void OnAnimationEvent(string eventName)
        {
            // Debug.Log("Animation Event: " + eventName);
            switch (eventName)
            {
                default:
                    break;
            }
        }

        public override void HurtResponse(HitData data)
        {
            if (ShellClosed)
            {
                BindableProperty<dynamic> shellHp = BoundEntity.GetCustomDataValue<dynamic>("shellHealthInfo","currentShellHealth");
                shellHp.Value -= data.Damage;
            }
            else
            {
                BoundEntity.TakeDamage(data.Damage,data.Attacker);
            }
        }
        
    }
}

