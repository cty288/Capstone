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
using Runtime.UI.NameTags;
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

        [field: ES3Serializable]
        public bool ShellStatus { get; set; } = true;
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

        protected override ICustomProperty[] OnRegisterCustomProperties() {
            return null;
        }

        
    }
    public class Boss1 : AbstractBossViewController<Boss1Entity>
    {
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

        // public override void HurtResponse(HitData data)
        // {
        //     base.HurtResponse(data);
        //     //new stuff like spawn blood effect
        // }
    }
}

