using Framework;
using System.Collections.Generic;
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

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return new[] {new AutoConfigCustomProperty("shellHealthInfo")};
        }

        
    }
    public class Boss1 : AbstractEnemyViewController<Boss1Entity>, IHurtResponder, IHitResponder
    {
        
        public int MaxShellHealth { get; }
        
        public int CurrentShellHealth { get; }
        [Header("HitResponder_Info")]
        [SerializeField] private int m_damage = 10;
        public Animator animator;
        public AnimationSMBManager animationSMBManager;
        public NavMeshAgent agent;
        public int Damage => m_damage;
        [SerializeField] private HitBox hitbox_roll;

        private List<GameObject> hitObjects = new List<GameObject>();
        private HitDetectorInfo hitDetectorInfo;
        [Header("Hurtresponder_Info")]
        [SerializeField] private List<HurtBox> hurtBoxes = new List<HurtBox>();

        public bool CheckHit(HitData data)
        {
            if (data.Hurtbox.Owner == gameObject) { return false; }
            else if (hitObjects.Contains(data.Hurtbox.Owner)) { return false; }
            else { return true; }
        }

        public bool CheckHurt(HitData data)
        {
            return true;
        }

        public void HitResponse(HitData data)
        {
            hitObjects.Add(data.Hurtbox.Owner);
        }

        public void HurtResponse(HitData data)
        {
            BoundEntity.TakeDamage(data.Damage,null);
        }

        protected override void OnEntityDie(IBelongToFaction damagedealer)
        {
            throw new System.NotImplementedException();
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
            hurtBoxes = new List<HurtBox>(GetComponentsInChildren<HurtBox>());
            foreach (HurtBox hurtBox in hurtBoxes)
            {
                hurtBox.HurtResponder = this;
            }

            //Animation-related.
            // animator = GetComponent<Animator>();
            animationSMBManager = GetComponent<AnimationSMBManager>();
            animationSMBManager.Event.AddListener(OnAnimationEvent);

            //Collision-related.
            hitbox_roll.HitResponder = this;
            hitDetectorInfo = new HitDetectorInfo();

        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer)
        {
            throw new System.NotImplementedException();
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
    }
}

